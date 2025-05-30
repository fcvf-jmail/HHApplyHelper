> 
git clone https://github.com/fcvf-jmail/HHApplyHelper
cd HHApplyHelper
dotnet restore
brew install --cask powershell
pwsh bin/Debug/net9.0/playwright.ps1 CHANGE YOUR .NET VERSION
dotnet run