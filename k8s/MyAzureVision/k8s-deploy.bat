#建立secret 
kubectl create secret generic azuresecret --from-literal=azure_api_key=66a24cc96ae74a88b27ffacc732a364e --from-literal=azure_api_endpoint=https://azureimg727.cognitiveservices.azure.com/

call gcloud config set project my-kubernates-398208
docker build -t michaelfangtw/myazurevision:1.0 .
docker push michaelfangtw/myazurevision:1.0
kubectl delete deployment myazurevision  
kubectl create deployment myazurevision  --image=michaelfangtw/myazurevision:1.0
kubectl delete service myazurevision
kubectl expose deployment myazurevision --type=LoadBalancer --port=80 --target-port=8080
kubectl get svc --watch
