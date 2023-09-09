for pid in  $(ps aux | grep "dotnet myAzureVision.dll" | awk '{print $2}') ; do
        kill -9 $pid
        echo "kill -9 $pid"
        break
done

export ASPNETCORE_ENVIRONMENT=Development
echo ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT

nohup dotnet myAzureVision.dll > myAzureVision.log  2>&1 &
tail myAzureVision.log -f
