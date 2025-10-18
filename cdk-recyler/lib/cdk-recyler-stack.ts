import * as cdk from 'aws-cdk-lib';
import { Distribution, ViewerProtocolPolicy } from 'aws-cdk-lib/aws-cloudfront';
import { S3BucketOrigin } from 'aws-cdk-lib/aws-cloudfront-origins';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as s3 from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';

export class CdkRecylerStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
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
      ec2.Port.allTraffic(),
      'Make the DB Publically accesible'
    )


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
  }
}
  