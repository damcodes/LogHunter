# Log Hunter

## Setup

.NET 8 is required on the machine you're running the app on. 

You must publish the executable to the machine you're running LogHunter on. Follow these steps to publish:

1. At the moment, LogHunter must be used on the machine of the environment you're trying to search through.  

   On your local machine, add the following environment variable:  
   
    - Variable Name: ASPNETCORE_ENVIRONMENT
    - Variable Value: Development  
    
   You'll  need to restart your machine after adding the environment variable.
   
2. Clone the LogHunter repo from [azure repos](https://dev.azure.com/fcaspa/_git/LogHunter%202)
3. Run `dotnet publish -c Release` which will build the app executable at `bin/Release/net8.0/publish/LogHunter.exe`
4. Copy `LogHunter.exe` to some desired location on your machine

## Executing a Hunt

### Interactive Mode

1. Run `<path_to_executable>/LogHunter.exe` in a terminal or click on LogHunter.exe from file explorer
2. Follow the prompts to build your query
    - **Apps**: Enter selection(s) as a comma separated list or hit enter to default to all apps  
        Currently, the only compatible apps are:  
        - Plutus
        - ChromeriverSyncService
        - CobblestoneSyncService
    - **LogLevel**: Enter selection(s) as comma separated list or hit enter to default to all log levels
    - **Callsite**: Enter the exact callsite you're searching for or hit enter to exclude from query
    - **Start Date/End Date**: 
        - Enter date part in one of the following formats:
            - dd-mm-YYYY
            - d-m-YYYY
            - dd-m-YYYY
            - d-mm-YYYY
        - Enter time part in the following format:
            - HH:MM:ss [AM/PM]
            - Time can be 24 hour time or 12 hour time with AM/PM
        - Start date will default to 72 hours ago
        - End date will default to right now
    - **TransactionId**: Enter the transaction ID to search for (must be a valid GUID) or hit enter to omit
    - **UserId**: Enter the User object ID to search for or hit enter to omit
    - **Message**: Enter the exception message to search for or hit enter to omit
3. LogHunter will notify you if it captured logs that satisfy your query and give you the file path to where the logs were dumped.

### Command Line Mode

In a terminal, run the following command
```
<path_to_executable>/LogHunter.exe [-a | --apps] <apps> [-s | --start] <start> [-e | --end] <end> [-l | --level] <levels> [-cs | --callsite] <callsite> [-m | --message] <message> [-tid | --transaction-id] <tid> [-uid | --user-id] <uid>
```

#### Start Date/End Date
Enter date part in one of the following formats:
- dd-mm-YYYY
- d-m-YYYY
- dd-m-YYYY
- d-mm-YYYY  

Enter time part in the following format:
- HH:MM:ss [AM/PM]
- Time can be 24 hour time or 12 hour time with AM/PM



Start Date will default to 72 hours ago if you don't provide a value.  
End Date will default to right now if you don't provide a value.

If you enter date and time with a space (i.e "02/27/2024 12:15:00"), you need to enclose the value in quotes:
```<path_to_executable>/LogHunter.exe -s "02/27/2024 12:15:00"```  
#### Apps
Input app(s) as a comma separated list of apps with no spaces. This IS case sensitive.  
```<path_to_executable>/LogHunter.exe --apps Plutus,ChromeriverSyncService```
#### LogLevel
Input log level(s) as a comma separated list of levels with no spaces. This is case insensitive.  
```<path_to_executable>/LogHunter.exe -l info,warn,critical```
