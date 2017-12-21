# IoTEdgeV2-SmartCitySensor-Sample

Microsoft IoT Edge V2 Public Preview: End to end sample in .NET Core 2.0 / C#

This guide assumes you are using Visual Studio Code to build and deploy the Linux based Docker containers written C# / .Net Core 2.0 which will provide the functionality for the Azure Iot Edge V2 as Edge modules. Check [this article](https://blogs.msdn.microsoft.com/visualstudio/2017/12/12/easily-create-iot-edge-custom-modules-with-visual-studio-code/) for more information on creating IoT Edge custom modules with Visual Studio Code.

The first project, [SmartCitySensor](./SmartCitySensor) simulates a sensor that delivers measurements every 2 seconds:

```JSON
{
    "SensorName":"TestSensor",
    "NoiseLevel":96,
    "AirQuality":"Good",
    "AirPressure":1228,
    "Timecreated":"2017-12-21T15:48:23.4456483+00:00"
}
```

it also simulates a "cheap" air pressure sensor that sometimes delivers zero-values. { "AirPressure":0 }

The second project, [FilterModule](./FilterModule) takes the readings from *SmartCitySensor* and filters the readings that contain invalid / zero *AirPressure* values.

## Setting up IoT Edge V2 Preview on your system

Windows:
Follow the [Deploy Azure IoT Edge on a simulated device in Windows - preview](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-simulate-device-windows) tutorial until *Step 3* to set up IoT Edge V2 Preview on your Windows System.

Linux:
Follow the [Deploy Azure IoT Edge on a simulated device in Linux - preview](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-simulate-device-linux) tutorial until *Step 3*  to set up IoT Edge V2 Preview on yourLinux System.

## Deploying the SmartCitySensor Module on IoT Edge V2

These steps have been adapted from the tutorial [Develop and deploy a C# IoT Edge module to your simulated device - preview](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-csharp-module).

### Prerequisites

- [Visual Studio Code](https://code.visualstudio.com/)
- [Azure IoT Edge extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-edge)
- [C# for Visual Studio Code (powered by OmniSharp) extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
- [Docker](https://docs.docker.com/engine/installation/) on the same computer that has Visual Studio Code. The Community Edition (CE) is - sufficient for this tutorial. 
- [.NET Core 2.0 SDK](https://www.microsoft.com/net/core#windowscmd)

### Create a Container Registry

You will use the Azure IoT Edge extension for VS Code to build the SmartCitySensor module and create a *container image* from the files. Then you push this image to a *container registry* that stores and manages your images. Finally, you deploy your image from your registry to run on your IoT Edge device.

You can use any Docker-compatible registry for this tutorial. Two popular Docker registry services available in the cloud are [Azure Container Registry](https://docs.microsoft.com/azure/container-registry/) and [Docker Hub](https://docs.docker.com/docker-hub/repos/#viewing-repository-tags). This text uses Azure Container Registry.

- In the [Azure portal](https://portal.azure.com/), select *Create a resource > Containers > Azure Container Registry*.
- Give your registry a name, choose a subscription, choose a resource group, and set the SKU to *Basic*.
- Select *Create*.
- Once your container registry is created, navigate to it and select *Access keys*.
- Toggle *Admin user* to *Enable*.
- Copy the values for *Login server, Username*, and *Password*. You'll use these values later.

### Deploy the SmartCitySensor Module to your Container Registry

Open the [SmartCitySensor](./SmartCitySensor) project in Visual Studio Code.

To build the project, right-click the *FilterModule.csproj* file in the Explorer and click *Build IoT Edge module*. This process compiles the module and exports the binary and its dependencies into a folder that is used to create a Docker image.

In VS Code explorer, expand the *Docker* folder. Then expand the folder for your container platform, either [linux-x64](./SmartCitySensor/Docker/linux-x64) (to run on a PC), [linux-arm32v7](./SmartCitySensor/Docker/linux-arm32v7) (to run on a Raspberry Pi) or [windows-nano](./SmartCitySensor/Docker/windows-nano).

Right-click the *Dockerfile* file and click *Build IoT Edge module Docker image*.

In the *Select Folder* window, either browse to or enter ```./bin/Debug/netcoreapp2.0/publish```. Click *Select Folder as EXE_DIR*.

in the pop-up text box at the top of the VS Code window, enter the image name. For example: ```<your container registry address>/smartcitysensor:latest```. The container registry address is the same as the login server that you copied from your registry. It should be in the form of ```<your container registry name>.azurecr.io```.

Sign in to Docker by entering the following command in the VS Code integrated terminal:

```
docker login -u <username> -p <password> <Login server>
```

Use the user name, password, and login server that you copied from your Azure container registry when you created it.

Push the image to your Docker repository. Select *View > Command Palette* and search for the *Edge: Push IoT Edge module Docker image* menu command. Enter the image name in the pop-up text box at the top of the VS Code window. Use the same image name you used in the previous step.

### Configure your Azure IoT Edge V2 Device to be able to connect to your Container Registry

Add the credentials for your registry to the Edge runtime on the computer where you are running your Edge device. These credentials give the runtime access to pull the container.

For Windows, run:
```
iotedgectl login --address <your container registry address> --username <username> --password <password> 
```

For Linux, run:
```
sudo iotedgectl login --address <your container registry address> --username <username> --password <password> 
```

### Deploy the SmartCitySensor IoT Edge Module to your IoT Edge device

Now use Azure IoT Edge to deploy the module to your IoT Edge device from the cloud.

- In the [Azure portal](https://portal.azure.com/), navigate to your IoT hub.
- Go to *IoT Edge (preview)* and select your IoT Edge device.
- Select *Set Modules*.
- Select *Add IoT Edge Module*.
- In the *Name* field, enter ```smartCitySensor```. 
- In the *Image URI* field, enter the address of your container in your container registry ```<your container registry address>/smartcitysensor:latest```.
- Leave the other settings unchanged, and select *Save*.
- Back in the Add modules step, select *Next*.
- In the Specify routes step, select *Next*.
- In the Review template step, select *Submit*.
- Return to the device details page and select *Refresh*. You should see the new *smartCitySensor* module running along the *IoT Edge runtime*.

### View the generated data

Open the command prompt on the computer running your simulated device again. Confirm that the module deployed from the cloud is running on your IoT Edge device by running:

```
sudo docker ps
```

View the messages being sent from the smartCitySensor module to the cloud:
 
```
sudo docker logs -f smartCitySensor
```

You can also view the telemetry the device is sending by using the [IoT Hub explorer tool](https://github.com/azure/iothub-explorer).

### Creating and publishing the FilterModule Docker container to your Container Registry

Repeat the above steps to create the Docker container for the [FilterModule](./FilterModule) project.

Publish it to your Docker Registry as ```<your container registry address>/filtermodule:latest```.

### Deploy the FilterModule IoT Edge Module to your IoT Edge device

- In the [Azure portal](https://portal.azure.com/), navigate to your IoT hub.
- Go to *IoT Edge (preview)* and select your IoT Edge device.
- Select *Set Modules*.
- Select *Add IoT Edge Module*.
- In the *Name* field, enter ```filterModule```. 
- In the *Image URI* field, enter the address of your container in your container registry ```<your container registry address>/filtermodule:latest```.
- Check the *Enable* box so that you can edit the module twin.
- Replace the JSON in the text box for the module twin with the following JSON:

```JSON
{
   "properties.desired":{
      "AirPressureThreshold":1
   }
}
```

- Select *Save*.
- Back in the Add modules step, select *Next*.
- In the *Specify Routes* step, copy the JSON below into the text box. Modules publish all messages to the Edge runtime. Declarative rules in the runtime define where the messages flow. We need two routes. The first route transports messages from the SmartCitySensor to the FilterModule via the "input1" endpoint, which is the endpoint that is configured with the *FilterMessages* handler. The second route transports messages from the FilterModule to IoT Hub. In this route, "upstream" is a special destination that tells Edge Hub to send messages to IoT Hub.

```JSON
{
    "routes": {
        "sensorToFilter": "FROM /messages/modules/smartCitySensor/outputs/output1 INTO BrokeredEndpoint(\"/modules/filterModule/inputs/input1\")",
        "filterToIoTHub": "FROM /messages/modules/filterModule/outputs/output1 INTO $upstream"
  }
}
```

- In the Review template step, select *Submit*.
- Return to the device details page and select *Refresh*. You should see the new *filterModule* running along with the *smartCitySensor* module and the *IoT Edge runtime*.

### View the generated data

Open the command prompt on the computer running your simulated device again. Confirm that both modules deployed from the cloud are running on your IoT Edge device by running:

```
sudo docker ps
```

View the messages being sent from the filterModule module to the cloud:
 
 ```
 sudo docker logs -f filterModule
 ```

Again, you can also view the telemetry the device is sending by using the [IoT Hub explorer tool](https://github.com/azure/iothub-explorer).
