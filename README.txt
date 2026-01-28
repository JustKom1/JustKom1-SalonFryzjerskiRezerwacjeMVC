Aby działały maile (confirm/reset), skonfiguruj EmailSettings w user-secrets albo zmiennych środowiskowych.


dotnet user-secrets init
dotnet user-secrets set "EmailSettings:Host" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:Port" "587"
dotnet user-secrets set "EmailSettings:UserName" "twojmail@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "TWOJE_HASLO_LUB_APP_PASSWORD"
dotnet user-secrets set "EmailSettings:From" "twoijmail@gmail.com"
