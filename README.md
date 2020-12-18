# epi-utilities-devicemonitor
Device Monitor Class - For use with and without Dynfusion



``` json

{
    "key" : "DevMon-1",
    "name" : "Device Monitor",
    "group" : "api",
    "type" : "devicemonitor",
    "properties" : 
    {
        "LogToDeviceKeys": 
        [
            "DynFusion01"
        ],
        "Devices" : 
        {
            "Device1": // Key can be any string. 
            {
                "name" : "Test 1", // Name that will be used in log and on the bridge 
                "warningTimeout" : 60, // Timeout until warning, Required for Simpl devices only
                "errorTimeout" : 180, // Timeout until error, Required for simple devices only 
                "logToProcessor": true, // Log warnings and errors to processor, not yet implemented 
                "logToDevices": true, // Log errors to Essentials devices in the "LogToDeviceKeys" array
                "joinNumber": 1 // Bridge join number 
            },

            "EssentialsDevice1":
            {
                "name": "Display01",
                "deviceKey" : "panasonicDisplay01", // Essentials based devices require a key for the device. Devices without keys will be created as Simpl devices. 
                "logToProcessor": true,
                "logToDevices": true,
                "joinNumber": 3
            }
        }
    }
}

'''
