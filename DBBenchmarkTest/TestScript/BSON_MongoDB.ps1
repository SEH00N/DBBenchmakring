$script = 'sudo systemctl start mongod && sleep 3 && mongosh mongodb://localhost:7777/test < $(pwd)/HelperScript/init_mongodb.js && sudo systemctl stop mongod'

wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "BSON_MongoDB" -TestNumber "1"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "BSON_MongoDB" -TestNumber "1"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "BSON_MongoDB" -TestNumber "2"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "BSON_MongoDB" -TestNumber "3"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "BSON_MongoDB" -TestNumber "4"
wsl -d ubuntu-24.04 -- bash -c "$script"
.\TestScript\RestartServiceAndRunTest.ps1 -TestCase "BSON_MongoDB" -TestNumber "5"