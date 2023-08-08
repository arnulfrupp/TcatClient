# TCAT Client

Client for Thread commissioning using TCAT

## Project 1: TcatMaui (MAUI)

MAUI based client for Thread commissioning using TCAT
Runs on Windows, iOS, Android, MacOS (Mac Catalyst - untested)

### Setup

Works best with Visual Studio.
Load solution and RUN. Follow instructions how to setup devicws.


## Project 2: TcatCli (CLI)

Command line client for Thread commissioning using TCAT.
Works best with command line development tools.

### Setup

#### Install dotnet (all OS)

**Check if dotnet is installed already**

```
dotnet --list-sdks
```

**If not, follow:**
[https://learn.microsoft.com/de-de/dotnet/core/install/linux](https://learn.microsoft.com/de-de/dotnet/core/install/linux)

#### Install dotnet (Ubutu 20.04)

**Install packet:**

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

**Install dotnet SDK (including runtime)**

```
sudo apt-get update
sudo apt-get install -y dotnet-sdk-7.0
```

#### Setup (Ubutu 22.04)

**Install dotnet SDK (including runtime)**

```
sudo apt-get update
sudo apt-get install -y dotnet-sdk-7.0
```

#### Check Bluetooth

**Check Bluetooth is running and which version is installed**

```
systemctl status bluetooth
bluetoothd -v
```

**On some computers Bluetooth needs to be unblocked using**

```
sudo rfkill unblock bluetooth
sudo systemctl restart bluetooth
```

### Compile code

**In folder TcatCli:**

```
dotnet run
or:
dotnet run Thread
```