param (
    [string] $TestCase,
    [string] $TestNumber
)

# 0. 프로세스 시작
$TargetService=""
switch ($TestCase) {
    "JSON_Redis" {
        $TargetService="redis-server"
        break
    }
    "MemoryPack_Redis" {
        $TargetService="redis-server"
        break
    }
    "JSON_MySQL" {
        $TargetService="mysql"
        break
    }
    "MemoryPack_MySQL" {
        $TargetService="mysql"
        break
    }
    "BSON_MongoDB" {
        $TargetService="mongod"
        break
    }
    default {
        exit
    }
}
Write-Host "[RestartServiceAndRunTest] TargetService : $TargetService"

# 1. 모든 WSL 인스턴스 종료
Write-Host "Shutting Down WSL..."
wsl --shutdown

# 2. 리소스 회수 대기
for ($i = 5; $i -gt 0; $i--) {
    Write-Host "`rWait For Clear WSL Resources in $i seconds..." -NoNewline
    Start-Sleep -Seconds 1
}

Write-Host "`rWait For Clear WSL Resources...                                                      "

# 3. WSL 실행하며 Redis 서비스 실행
Write-Host "Starting $TargetService Service in WSL..."
Start-Process "wsl" -ArgumentList "-d ubuntu-24.04 -- bash -c 'sudo systemctl start $TargetService && sleep infinity'"

# 4. 서비스 업타임 대기
Write-Host "Wait For Service Uptime for 5 Seconds..."
Start-Sleep -Seconds 5

# 4. 테스트 실행
Write-Host "Start Benchmark Test"
.\bin\Debug\net8.0\DBBenchmarkTest.exe $TestCase $TestNumber

# 5. WSL 종료
Write-Host "Release WSL..."
wsl --shutdown