# PrinterStatusLogger
Program to collect data from printers in your network.
Allows you to automatically get data from printer web interface.

## Capabilities
Report
- Log in console output
- Log in `syslog` file
- Alert through email

Printer scan
- Get toner level
- List unavaliable printers

## Usage
Run from command line:

`printerstatuslogger.exe [arguments]`

Avaliable arguments:
- `-u` User Mode - Program can ask user certain things (like creating default config), and wait for user input
- `-na` No Alert Mode - Disables whole Alerter module
- `-l` Model List - Only lists loaded printer models and then exits

## Configuration

### Basic Configuration
Program configuration is located in `Config` directory.
It is based on 2 files.
- `alerter.cfg` - Config of alerter module
  Smtp server and port, message recipients and alerter rules 
- `printers.cfg` - List of printers to scan
These files will be created from embedded defaults (require run program with User Mode).

### Smtp Credentials
Credentials for smtp server to authenticate are securely stored in PasswordVault (Windows Credential Manager `control keymgr.dll`).

### Printer Models
All avaliable printer models are located in `Models` directory.
Each file represents each model.
You can download models from this repo or create model file by yourself using following template:
```
id=[Unique model id]
name=[Name of printer]
readtonerlevelpath=[Path after web interface address to find toner level]
readtonerlevelregex=[Regex to find toner level]
```

## TODO
- Reports Module - to report data in csv files
- GetTonerLevel() Improvement - better response from function when error occurs
- Printer Scan Improvement - multitasking (to not wait when f.ex. when cannot connect to address)
- ReadConfig() - rewrite to create kvp's list for better config reading
- Logger
  - Logs in syslog format
  - Availability to connect to syslog server