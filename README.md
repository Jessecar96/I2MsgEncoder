# I2MsgEncoder

**Released for reference and historical purposes only. This code no longer works.**

This is the original I2MsgEncoder, created in 2019 as the first program to send data to an Intellistar 2. It uses TWC's deprecated DSX API, and requires an older software version on the I2 to work.

I am releasing it now under the GPL 3.0 license to use as a reference in making your own projects.

# I2Lib

Library for sending UDP messages, can be used in other projects to send UDP messages.

## Goals

 - Run in the background to send conditions and radar frames automatically. 
 - Configuration file when to run presentations.

## TODO
 - Send satellite images?
 - Scheduling system for when to cue presentations?
 - Marine bulletins
 - Climatology records
 - Holiday backgrounds

## Done
 - Sending UDP packets to MsgIngester.
 - Processing and sending weather data from DSX.
 - Processing and sending tile-based radar and satrad from TWC's tile server
 - Sending ad crawls.
 - Holiday updating
 - Daypart vocalLocal using holiday names

## Records
- [ ] ADRecord - Airport Delays
- [ ] ClimatologyRecord - Record highs/lows
- [x] DDRecord - Daypart Forecast
- [x] DFRecord - Daily Forecast (7 Day Forecast)
- [x] ESRecord - Air Quality
- [x] IDRecord - Pollen
- [x] MORecord - Current Conditions
- [ ] SKRecord - Ski Conditions (Not used)
- [x] TIRecord - Tides
- [ ] TFRecord - Traffic Flow
- [ ] TNRecord - Traffic Incidents
- [ ] WBRecord - Buoy Data (Not used)
- [ ] WMRecord - Marine Forecast


## Setup

 1. This software requires a separate NIC with an IP address of `10.100.102.10` The I2 has Windows firewall rules in place to only allow UDP traffic in from that IP.
 2. Rename config.example.json to config.json and edit your headend id.
 3. Place your MachineProductCfg.xml file in the same directory as I2MsgEncoder
