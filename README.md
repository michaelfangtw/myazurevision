# myazurevision
```
 a asp.net core 6.0 project using azure vision api to detect images tag
 appengine/* deploy to gcp appengine
 k8s/*       depoly to gcp kubernates
```

## appengine
### Step 1
```
install gcloud sdk
```
### Step2
edit app.yaml
```
runtime: aspnetcore
env: flex
service: app-azure-vision-service

runtime_config:
  operating_system: ubuntu22

# This sample incurs costs to run on the App Engine flexible environment. 
# The settings below are to reduce costs during testing and are not appropriate
# for production use. For more information, see:
# https://cloud.google.com/appengine/docs/flexible/dotnet/configuring-your-app-with-app-yaml
manual_scaling:
  instances: 1
resources:
  cpu: 1
  memory_gb: 0.5
  disk_size_gb: 10

env_variables:
  azure_api_key: ''
  azure_api_endpoint: ''
```
### step 3
```
gcloud config set project xxxxx
# deploy service : app-azure-vision-service
gcloud app deploy --version=v1
```
---
## k8s
### Step 1: publish 
  publish dotnet core project
  and copy pubish folder to k8s/MyAzureVision/publish
### Step 2: Dockerfile
```
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY publish/ ./
EXPOSE 8080/tcp 
ENV ASPNETCORE_URLS http://*:8080
ENTRYPOINT ["dotnet", "MyAzureVision.dll"]
```
## step3. create secret
```
kubectl create secret generic azuresecret --from-literal=azure_api_key=xxx --from-literal=azure_api_endpoint=https://xxxxx.cognitiveservices.azure.com/
```

## step4. deploy to GKE
```
#get credentials
gcloud container clusters get-credentials my-cluster --zone us-central1-c --project  my-kubernates-xxxx

#set your prject
gcloud config set project my-kubernates-398208

#build images
docker build -t michaelfangtw/myazurevision:1.0 .

#push to docker hub
docker push michaelfangtw/myazurevision:1.0
kubectl delete deployment myazurevision  

#k8s deploy workloads images
kubectl create deployment myazurevision  --image=michaelfangtw/myazurevision:1.0

#export workloads to public ip / port
kubectl delete service myazurevision
kubectl expose deployment myazurevision --type=LoadBalancer --port=80 --target-port=8080
kubectl get svc --watch
```

## Step5. modify myazurevision.yaml to mapping  secret value to enviorment variables
```
      containers:
        - env:
            - name: azure_api_key
              valueFrom:
                secretKeyRef:
                  name: azuresecret
                  key: azure_api_key
            - name: azure_api_endpoint
              valueFrom:
                secretKeyRef:
                  name: azuresecret
                  key: azure_api_endpoint
          image: michaelfangtw/myazurevision:1.2v
```
