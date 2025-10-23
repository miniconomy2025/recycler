import * as cdk from 'aws-cdk-lib';
import * as certificatemanager from 'aws-cdk-lib/aws-certificatemanager';
import { Distribution, ViewerProtocolPolicy } from 'aws-cdk-lib/aws-cloudfront';
import { S3Origin } from 'aws-cdk-lib/aws-cloudfront-origins';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as iam from 'aws-cdk-lib/aws-iam'; // Add IAM import for bucket policy
import * as route53 from 'aws-cdk-lib/aws-route53';
import * as s3 from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';
import * as targets from 'aws-cdk-lib/aws-route53-targets'

export class CdkRecylerStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    //CERTIFICATE AND ROUTING STUFF
    const domainName = 'susnet.co.za';
    const apiSubDomain = 'api.recycler';

    const hostedZone = new route53.HostedZone(this, 'HostedZone', {
      zoneName: domainName
    })

    // Add this after your hostedZone creation
    const certificate = new certificatemanager.Certificate(this, 'Certificate', {
      domainName: domainName,
      subjectAlternativeNames: [`*.${domainName}`],
      validation: certificatemanager.CertificateValidation.fromDns(hostedZone),
    })

    //GENERAL
    const generalVpc = new ec2.Vpc(this, 'generalVpc', {
      maxAzs: 1, // Only 1 AZ = cheapest
      natGateways: 0, // NAT gateways are expensive – we remove them (who's we ChatGPT you mean team recycler, us, removes them?)
      subnetConfiguration: [
        {
          name: 'public',
          subnetType: ec2.SubnetType.PUBLIC,
          cidrMask: 24,
        },
      ],
    });

    //SECURITY STUFF
    const securityGroup = new ec2.SecurityGroup(this, 'generalSG', {
      vpc: generalVpc,
      description: 'Allow HTTP/HTTPS access only',
      allowAllOutbound: true,
    });

    //TODO: MAYBE GET RID OF ALL TRAFFIC BUT LIKE CHILL FOR NOW
    securityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(80), "HTTP");
    securityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(443), "HTTPS");
    securityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(22), "SSH");
    securityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(5432), "POSTGRES"); // optional

    //FRONT ENED STUFF
    const websiteBucket = new s3.Bucket(this, 'WebsiteBucket', {
      bucketName: `recycler-website-${cdk.Stack.of(this).account}`,
      removalPolicy: cdk.RemovalPolicy.DESTROY, // Use RETAIN for production
      autoDeleteObjects: true, // Only use with DESTROY policy
      websiteIndexDocument: 'index.html',
      websiteErrorDocument: 'error.html',
      publicReadAccess: false, // CloudFront will access via OAI
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
    });

    // Add this after the S3 bucket
    const distribution = new Distribution(this, 'WebsiteDistribution', {
      defaultBehavior: {
        origin: new S3Origin(websiteBucket),
        viewerProtocolPolicy: ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
      },
      domainNames: [domainName],
      certificate: certificate,
      defaultRootObject: 'index.html',
    });

    // GRANT CLOUDFRONT ACCESS TO S3 BUCKET
      websiteBucket.addToResourcePolicy(new iam.PolicyStatement({
        actions: ['s3:GetObject'],
        resources: [websiteBucket.arnForObjects('*')],
        principals: [new iam.ServicePrincipal('cloudfront.amazonaws.com')],
        conditions: {
          StringEquals: {
            'AWS:SourceArn': `arn:aws:cloudfront::${cdk.Stack.of(this).account}:distribution/${distribution.distributionId}`,
          },
        },
      }));

    const websiteARecord = new route53.ARecord(this, 'websiteARecord', {
      zone: hostedZone,
      recordName: domainName,
      target: route53.RecordTarget.fromAlias(new targets.CloudFrontTarget(distribution)),
    });

    //BACKEND STUFF
    const ec2Instance = new ec2.Instance(this, "backend", {
      vpc: generalVpc,
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO), // cheapest
      machineImage: ec2.MachineImage.latestAmazonLinux2023(),
      securityGroup: securityGroup,
      vpcSubnets: { subnetType: ec2.SubnetType.PUBLIC },
      keyPair: ec2.KeyPair.fromKeyPairName(this, 'keyPair', 'ec2-key-pair')
    })

    const apiARecord = new route53.ARecord(this, 'ApiARecord', {
      zone: hostedZone,
      recordName: apiSubDomain,
      target: route53.RecordTarget.fromIpAddresses(ec2Instance.instancePublicIp),
    });
  }
}
