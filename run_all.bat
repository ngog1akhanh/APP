@echo off
echo Building the solution first to avoid file lock issues...
dotnet build TourGuide.Backend.slnx
if %ERRORLEVEL% neq 0 (
    echo Build failed! Please check the errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo Starting TourGuide Backend Applications...

echo Starting TourGuide.API...
start cmd /k "title TourGuide.API & dotnet run --project TourGuide.API\TourGuide.API.csproj --no-build"

echo Starting TourGuide.WebAdmin...
start cmd /k "title TourGuide.WebAdmin & dotnet run --project TourGuide.WebAdmin\TourGuide.WebAdmin.csproj --no-build"

echo Starting TourGuide.WebQR...
start cmd /k "title TourGuide.WebQR & dotnet run --project TourGuide.WebQR\TourGuide.WebQR.csproj --no-build"

echo All applications started!
