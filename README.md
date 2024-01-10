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

## Configuration
Program configuration is located in `Config` directory.
It is based on 2 files.
- `alerter.cfg` - Config of alerter module
- `printers.cfg` - List of printers to scan

## Printer Models
All avaliable printer models are located in `Models` directory.
Each file represents each model.
You can download models from this repo or create model file by yourself using following template:
```
id=[Unique model id]
name=[Name of printer]
readtonerlevelpath=[Path after web interface address to find toner level]
readtonerlevelregex=[Regex to find toner level]
```
