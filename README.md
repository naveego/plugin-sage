# plugin-sage

Regenerate publisher
```
~/.nuget/packages/grpc.tools/1.17.0/tools/linux_x64/protoc -I../ --csharp_out ./Publisher --grpc_out ./Publisher ../publisher.proto --plugin=protoc-gen-grpc=$HOME/.nuget/packages/grpc.tools/1.17.0/tools/linux_x64/grpc_csharp_plugin
```