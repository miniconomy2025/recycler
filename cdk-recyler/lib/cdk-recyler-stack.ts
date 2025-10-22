import * as cdk from 'aws-cdk-lib';
import { Distribution, ViewerProtocolPolicy } from 'aws-cdk-lib/aws-cloudfront';
import { S3BucketOrigin } from 'aws-cdk-lib/aws-cloudfront-origins';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as targets from 'aws-cdk-lib/aws-route53-targets';
import * as albtargets from 'aws-cdk-lib/aws-elasticloadbalancingv2-targets';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as s3 from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';
import * as route53 from 'aws-cdk-lib/aws-route53';
import * as certificatemanager from 'aws-cdk-lib/aws-certificatemanager';

export class CdkRecylerStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    //CERTIFICATE AND ROUTING STUFF
    const domainName = 'susnet.co.za';
    const apiSubDomain = 'api.recycler';

    const hostedZone = new route53.HostedZone(this, 'HostedZone', {
      zoneName: domainName
    })

    // const apiCertificate = new certificatemanager.Certificate(this, "ApiRecyclerCert", {
    //   domainName: [apiSubDomain, domainName].join('.'),
    //   validation: certificatemanager.CertificateValidation.fromDns(hostedZone),
    // });

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

    //BACKEND STUFF
    const ec2Instance = new ec2.Instance(this, "backend", {
      vpc: generalVpc,
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO), // cheapest
      machineImage: ec2.MachineImage.latestAmazonLinux2023(),
      securityGroup: securityGroup,
      vpcSubnets: { subnetType: ec2.SubnetType.PUBLIC },
      keyPair: ec2.KeyPair.fromKeyPairName(this, 'keyPair', 'ec2-key-pair')
    })

    // const apiARecord = new route53.ARecord(this, 'ApiARecord', {
    //   zone: hostedZone,
    //   recordName: apiSubDomain,
    //   target: route53.RecordTarget.fromIpAddresses(ec2Instance.instancePublicIp),
    // });
  }
}
