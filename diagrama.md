# Diagrama de Arquitectura AWS — I See U

![Diagrama de Arquitectura AWS](diagrama.png)

## Recursos AWS

| Servicio | Recurso | Detalle |
|---|---|---|
| **VPC** | `10.0.0.0/16` | Red virtual con subnets públicas y privadas |
| **EKS** | `iseeu-cluster` | Orquestación Kubernetes |
| **ECR** | `namespace/iseeu-backend` | Repositorio de imágenes backend (.NET 10) |
| **ECR** | `namespace/iseeu-frontend` | Repositorio de imágenes frontend (NGINX) |
| **ELB** | LoadBalancer | Creado automáticamente por frontend Service |
| **EBS** | `postgres-pvc` (1Gi) | Volumen persistente para PostgreSQL |
| **IAM** | Roles | Node group, ECR pull, GitHub Actions |

## Subnets (5 públicas)

| AZ | CIDR | Tags |
|---|---|---|
| us-east-1a | `10.0.1.0/24` | `kubernetes.io/role/elb = 1`, `kubernetes.io/cluster/iseeu-cluster = shared` |
| us-east-1b | `10.0.2.0/24` | `kubernetes.io/role/elb = 1`, `kubernetes.io/cluster/iseeu-cluster = shared` |
| us-east-1c | `10.0.3.0/24` | `kubernetes.io/role/elb = 1`, `kubernetes.io/cluster/iseeu-cluster = shared` |
| us-east-1d | `10.0.4.0/24` | `kubernetes.io/role/elb = 1`, `kubernetes.io/cluster/iseeu-cluster = shared` |
| us-east-1e | `10.0.5.0/24` | `kubernetes.io/role/elb = 1`, `kubernetes.io/cluster/iseeu-cluster = shared` |

## Recursos Kubernetes (namespace: `iseeu`)

| Tipo | Nombre | Detalles |
|---|---|---|
| Deployment | `postgres` | 1 replica, PostgreSQL 16, puerto 5432 |
| Service | `postgres` | ClusterIP :5432 |
| Secret | `postgres-secret` | Credenciales PostgreSQL |
| PVC | `postgres-pvc` | 1Gi ReadWriteOnce (EBS) |
| Deployment | `backend` | 2-10 replicas, .NET 10, HPA 70% CPU |
| Service | `backend` | ClusterIP :5000 |
| Deployment | `frontend` | 2-6 replicas, NGINX, HPA 60% CPU |
| Service | `frontend` | LoadBalancer :80 |

## Flujo

```
Usuario → ELB → Frontend (NGINX :80) → /api/* → Backend (.NET :5000) → PostgreSQL (:5432)
```

## CI/CD (GitHub Actions)

```
push main → Build Docker → Push ECR → kubectl apply → Deploy EKS
```
