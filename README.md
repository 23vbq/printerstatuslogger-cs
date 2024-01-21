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
- List scan errors

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
- `alerter.cfg` - Config of alerter module | [default config](PrinterStatusLogger/Config/DefaultConfig/alerter.cfg.def) |

  Smtp server and port, message recipients and alerter rules 
- `printers.cfg` - List of printers to scan | [default config](PrinterStatusLogger/Config/DefaultConfig/printers.cfg.def) |

  These files will be created from embedded files (require run program with User Mode).

### Smtp Credentials
Credentials for smtp server to authenticate are securely stored in PasswordVault (Windows Credential Manager `control keymgr.dll`).
They are stored in Web Credentials with URL reference of `PrinterStatusLogger_Alerter`.
They are loaded to memory on Alerter module initialization.

To setup credentials you need to run program with User Mode.
If you need to edit credentials you need to delete them from Windows Credential Manager and then run program with User Mode to setup credentials again.

### Printer Models
All avaliable printer models are located in `Models` directory.
Each file represents each model.
You can download models from this repo or create model file by yourself using following template:
```
id=[Unique model id]
name=[Name of printer]
readtonerlevelpath=[Path after web interface address to find toner level]
readtonerlevelregex=[Regex to find toner level [0-9]{1,3}]
```

## TODO
- Reports Module - to report data in csv files
- ~~GetTonerLevel() Improvement - better response from function when error occurs~~ - implemented need testing
- Printer Scan Improvement
  - Multitasking (to not wait when f.ex. when cannot connect to address)
  - Try to ping when http not response
- ReadConfig() - rewrite to create kvp's list for better config reading
- Logger
  - Logs in syslog format
  - Availability to connect to syslog server
- Versions
  - Installer
  - Portable *(maybe for this option to use credentials every run, instead storing in keymgr.dll)*
- Option to download models form github directly in program
- Combine all Add__Alert functions to one function
- Run arguments
  - Verbose mode that logs additional info
  - Help
- *Config as class*
- *ExchangeOnline Server support - in future*
- *Getting prints counter - in future*
