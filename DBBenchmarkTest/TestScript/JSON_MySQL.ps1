$script = 'sudo systemctl start mysql && sleep 3 && mysql -u test -ptest test < $(pwd)/HelperScript/init_mysql.sql && sudo systemctl stop mysql'

wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "JSON_MySQL" -TestNumber "1"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "JSON_MySQL" -TestNumber "1"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "JSON_MySQL" -TestNumber "2"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "JSON_MySQL" -TestNumber "3"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "JSON_MySQL" -TestNumber "4"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "JSON_MySQL" -TestNumber "5"