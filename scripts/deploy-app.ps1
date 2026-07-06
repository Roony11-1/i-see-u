<#
.SINOPSIS
  Construye, sube a ECR y despliega la app en EKS.
  Asume que el cluster EKS ya existe y esta ACTIVE.

.DESCRIPCION
  1. Login a ECR
  2. Build + push imagenes Docker
  3. Configura kubectl
  4. Aplica manifests K8s
  5. Verifica el deploy

.EJEMPLO
  .\scripts\deploy-app.ps1
#>

param(
    [switch]$skipBuild
)

# ============================================
# 1. Cargar .env
# ============================================
$envFile = Join-Path (Get-Location) ".env"
if (-not (Test-Path $envFile)) {
    Write-Host "[ERROR] No se encuentra .env en la raiz del proyecto." -ForegroundColor Red
    exit 1
}

Write-Host "[1/6] Cargando configuracion desde .env ..." -ForegroundColor Cyan
Get-Content $envFile | ForEach-Object {
    if ($_ -match "^\s*([^#=]+)=(.*)$") {
        Set-Item -Path "env:$($matches[1].Trim())" -Value $matches[2].Trim()
    }
}

$AWS_REGION       = $env:AWS_REGION
$AWS_ACCOUNT_ID   = $env:AWS_ACCOUNT_ID
$EKS_CLUSTER_NAME = $env:EKS_CLUSTER_NAME
$EKS_NAMESPACE    = $env:EKS_NAMESPACE
$ECR_BACKEND      = $env:ECR_REPO_BACKEND
$ECR_FRONTEND     = $env:ECR_REPO_FRONTEND
$IMAGE_TAG        = $env:IMAGE_TAG
$ECR_REGISTRY     = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

Write-Host "  Region: $AWS_REGION | Cluster: $EKS_CLUSTER_NAME | Tag: $IMAGE_TAG"

# ============================================
# 2. Login a ECR
# ============================================
Write-Host "[2/6] Login a ECR ..." -ForegroundColor Cyan
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_REGISTRY
if ($LASTEXITCODE -ne 0) { Write-Host "[ERROR] Login ECR fallo" -ForegroundColor Red; exit 1 }

# ============================================
# 3. Build + push imagenes (opcional con -skipBuild)
# ============================================
if (-not $skipBuild) {
    Write-Host "[3/6] Construyendo backend ..." -ForegroundColor Cyan
    docker build -t $ECR_REGISTRY/$ECR_BACKEND`:$IMAGE_TAG ./backend
    if ($LASTEXITCODE -ne 0) { Write-Host "[ERROR] Build backend fallo" -ForegroundColor Red; exit 1 }

    Write-Host "  Pusheando backend ..."
    docker push $ECR_REGISTRY/$ECR_BACKEND`:$IMAGE_TAG
    docker tag $ECR_REGISTRY/$ECR_BACKEND`:$IMAGE_TAG $ECR_REGISTRY/$ECR_BACKEND`:latest
    docker push $ECR_REGISTRY/$ECR_BACKEND`:latest

    Write-Host "[3/6] Construyendo frontend ..." -ForegroundColor Cyan
    docker build -t $ECR_REGISTRY/$ECR_FRONTEND`:$IMAGE_TAG ./frontend
    if ($LASTEXITCODE -ne 0) { Write-Host "[ERROR] Build frontend fallo" -ForegroundColor Red; exit 1 }

    Write-Host "  Pusheando frontend ..."
    docker push $ECR_REGISTRY/$ECR_FRONTEND`:$IMAGE_TAG
    docker tag $ECR_REGISTRY/$ECR_FRONTEND`:$IMAGE_TAG $ECR_REGISTRY/$ECR_FRONTEND`:latest
    docker push $ECR_REGISTRY/$ECR_FRONTEND`:latest
} else {
    Write-Host "[3/6] Build omitido (-skipBuild). Usando imagenes existentes en ECR." -ForegroundColor Yellow
}

# ============================================
# 4. Configurar kubectl
# ============================================
Write-Host "[4/6] Configurando kubectl para EKS ..." -ForegroundColor Cyan
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME 2>$null
kubectl cluster-info 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] No se puede conectar al cluster '$EKS_CLUSTER_NAME'." -ForegroundColor Red
    Write-Host "  Verifica que el cluster exista y este ACTIVE en la consola AWS." -ForegroundColor Yellow
    exit 1
}
Write-Host "  Conectado a: $(kubectl config current-context)" -ForegroundColor Green

# ============================================
# 5. Aplicar manifests
# ============================================
Write-Host "[5/6] Aplicando manifests a Kubernetes ..." -ForegroundColor Cyan

kubectl apply --validate=false -f k8s/namespace.yaml
kubectl apply --validate=false -f k8s/postgres/
kubectl apply --validate=false -f k8s/backend/
kubectl apply --validate=false -f k8s/frontend/

kubectl set image deployment/backend backend=$ECR_REGISTRY/$ECR_BACKEND`:$IMAGE_TAG -n $EKS_NAMESPACE
kubectl set image deployment/frontend frontend=$ECR_REGISTRY/$ECR_FRONTEND`:$IMAGE_TAG -n $EKS_NAMESPACE

# ============================================
# 6. Verificar estado
# ============================================
Write-Host "[6/6] Verificando deploy ..." -ForegroundColor Cyan
kubectl rollout status deployment/backend -n $EKS_NAMESPACE
kubectl rollout status deployment/frontend -n $EKS_NAMESPACE

Write-Host "`n=== PODS ===" -ForegroundColor Cyan
kubectl get pods -n $EKS_NAMESPACE

Write-Host "`n=== SERVICIOS ===" -ForegroundColor Cyan
kubectl get svc -n $EKS_NAMESPACE

Write-Host "`n[LISTO]" -ForegroundColor Green
Write-Host "Entra al EXTERNAL-IP del servicio 'frontend' en tu navegador." -ForegroundColor Green
