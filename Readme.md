dotnet ef migrations list
dotnet ef migrations add InitialCreate
dotnet ef update

dotnet ef database drop -f