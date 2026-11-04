# Sony-HeadTracking-Unity-Script
use the built in head tracking in sony headphones in unity by reading the values provided by NicholasSlattery/sony-head-tracker

his project sends all tracking data to port 4243, so in unity i create a new thread to not interrupt the update function and safely retrieve the data from the port.
