# Terraform Starter (AWS)

This folder contains a starter Terraform structure for interview-ready infrastructure.

## Intended AWS mapping
- `modules/network`: VPC, subnets, internet/NAT gateways, routing, security groups
- `modules/app`: ECS/App Runner service, ECR image, CloudWatch logs, IAM role attachments
- `environments/dev`: Composition layer for development environment

## Next implementation steps
1. Add remote state backend (S3 + DynamoDB lock table)
2. Create RDS PostgreSQL module and connect security groups
3. Add Cognito user pool + app client
4. Add S3 + CloudFront for React hosting
5. Add Route53 + ACM for custom domain
