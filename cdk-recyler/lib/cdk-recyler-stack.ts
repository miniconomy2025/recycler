import * as cdk from 'aws-cdk-lib';
import { Distribution, ViewerProtocolPolicy } from 'aws-cdk-lib/aws-cloudfront';
import { S3BucketOrigin } from 'aws-cdk-lib/aws-cloudfront-origins';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as targets from 'aws-cdk-lib/aws-elasticloadbalancingv2-targets';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as s3 from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';
import * as route53 from 'aws-cdk-lib/aws-route53';
import * as route53Targets from 'aws-cdk-lib/aws-route53-targets';
import * as certificatemanager from 'aws-cdk-lib/aws-certificatemanager';

export class CdkRecylerStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    //CERTIFICATE STUFF
    const domainName = 'susnet.co.za';
    const apiSubDomain = 'api.recycler';

    const hostedZone = new route53.HostedZone(this, 'HostedZone', {
      zoneName: domainName
    })

    // const certificate = new certificatemanager.Certificate(this, 'Cert', {
    //   domainName: apiSubDomain,
    //   validation: certificatemanager.CertificateValidation.fromDns(hostedZone)
    // });

    //GENERAL
    const generalVpc = new ec2.Vpc(this, 'generalVpc', {
      maxAzs: 2,
    });

    const securityGroup = new ec2.SecurityGroup(this, 'generalSecurityGroup', {
      vpc: generalVpc,
      description: 'Allow DB access',
      allowAllOutbound: true,
      securityGroupName: 'general-security-group'
    });


    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(22),
      'Allow SSH access'
    );

    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(5000),
      'Allow access to application port'
    );

    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(80),
      'Allow HTTP access'
    );

    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(443),
      'Allow HTTPS access'
    );

    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(5432),
      'Allow DB access'
    );


    //DB STUFF
    const dbCredentials: rds.Credentials = rds.Credentials.fromGeneratedSecret('postGresDbConnection', {
      secretName: 'post-gres-db-connection'
    })

    const dbInstace = new rds.DatabaseInstance(this, 'recyclerPostgresInstance', {
      engine: rds.DatabaseInstanceEngine.postgres({ version: rds.PostgresEngineVersion.VER_17_5 }),
      instanceIdentifier: 'recycler-postgres-instance',
      vpc: generalVpc,
      backupRetention: cdk.Duration.days(0),
      credentials: dbCredentials,
      multiAz: false,
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO),
      deletionProtection: false,
      databaseName: 'recycler',
      allocatedStorage: 20,
      maxAllocatedStorage: 20,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      storageEncrypted: true,
      deleteAutomatedBackups: true,
      publiclyAccessible: true,
      vpcSubnets: {
        subnetType: ec2.SubnetType.PUBLIC
      },
      securityGroups: [securityGroup],
    });

    //FRONT END STUFF
    const recyclerBucket = new s3.Bucket(this, 'recycler-bucket', {
      bucketName: 'recycler-bucket-miniconomy',
      versioned: true,
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true
    });

    const frontEndDistribution = new Distribution(this, 'bucket-distribution', {
      defaultBehavior: {
        origin: S3BucketOrigin.withOriginAccessControl(recyclerBucket),
        viewerProtocolPolicy: ViewerProtocolPolicy.HTTPS_ONLY,
      },
      defaultRootObject: 'index.html',
      errorResponses: [
        {
          httpStatus: 403,
          responseHttpStatus: 200,
          responsePagePath: '/index.html',
        },
        {
          httpStatus: 404,
          responseHttpStatus: 200,
          responsePagePath: '/index.html',
        }
      ],
      enableIpv6: false,
    })


    //BACK-END STUFF
    const ec2Instance = new ec2.Instance(this, 'recyclerEC2Instance', {
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO),
      machineImage: ec2.MachineImage.latestAmazonLinux2023(),
      vpc: generalVpc,
      vpcSubnets: {
        subnetType: ec2.SubnetType.PUBLIC
      },
      securityGroup: securityGroup,
      keyName: 'ec2-key-pair', // Replace with actual key name or remove this line if not using SSH
    });

    const apiCertificate = new certificatemanager.Certificate(this, "ApiRecyclerCert", {
      domainName: "api.recycler.susnet.co.za",
      validation: certificatemanager.CertificateValidation.fromDns(hostedZone),
    });

    const alb = new elbv2.ApplicationLoadBalancer(this, 'ApplicationLoadBalancer', {
      vpc: generalVpc,
      internetFacing: true,
      securityGroup: securityGroup,
    });

    const targetGroup = new elbv2.ApplicationTargetGroup(this, 'TargetGroup', {
      vpc: generalVpc,
      port: 5000,
      protocol: elbv2.ApplicationProtocol.HTTP,
      targets: [new targets.InstanceTarget(ec2Instance)],
      healthCheck: {
        path: '/swagger/index.html',
        port: '5000'
      }
    });

    alb.addListener('HttpsListener', {
      port: 443,
      protocol: elbv2.ApplicationProtocol.HTTPS,
      certificates: [apiCertificate],
      defaultTargetGroups: [targetGroup]
    });

    alb.addListener('HttpListener', {
      port: 80,
      protocol: elbv2.ApplicationProtocol.HTTP,
      defaultAction: elbv2.ListenerAction.redirect({
        protocol: 'HTTPS',
        port: '443'
      })
    });

    const albARecord = new route53.ARecord(this, 'ALBRecord', {
      zone: hostedZone,
      target: route53.RecordTarget.fromAlias(new route53Targets.LoadBalancerTarget(alb)),
      recordName: 'api.recycler',
      ttl: cdk.Duration.minutes(5)
    });
  }
}
