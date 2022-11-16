# DistributedDatastore

## Local
```
cd DistributedDatastore
```

Start Leader
```
Options__IsLeader=True Kestrel__Endpoints__Http__Url="http://localhost:5000" dotnet run
```

Start Server1
```
Options__IsLeader=False Kestrel__Endpoints__Http__Url="http://localhost:5001" dotnet run
```

Start Server2
```
Options__IsLeader=False Kestrel__Endpoints__Http__Url="http://localhost:5002" dotnet run
```

## Docker
```
sudo docker compose up
```

(If you changed something in the code)
```
sudo docker compose down
sudo docker image rm distributeddatastore-server1
sudo docker image rm distributeddatastore-server2
sudo docker image rm distributeddatastore-server3
sudo docker compose up
```

