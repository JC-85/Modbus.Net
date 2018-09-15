Modbus.Net.Simple Oveview
===================
Modbus.Net.Simple is a fork of parallelbgls Modbus.Net library. The goals of this is as follows:
* Ease of use
* Minimal dependancys
* Linux-compatible using DotNet Core

About Modbus
-------------------
Wikipedia: http://en.wikipedia.org/modbus

Modbus is a open protocol used in factory automation and SCADA scenarios. It's suitable for reading and writing to bit-devices and microcontrollers such as PLCs and HMI's. It's widly supported but implementations of registers and coils differ between brands. This library tries to alleviate this by being flexible with register addressing.

Coil, discrete input, input register, holding register numbers and addresses
----------
Modbus reads and writes data from "device entities"

|Object type |	Access |	Size|
|---|---|---|
Coil	|Read-write	|1 bit
Discrete input	|Read-only	|1 bit
Input register	|Read-only	|16 bits
Holding register	|Read-write	|16 bits

It is important to make a distinction between entity number and entity address:

Entity numbers combine entity type and entity location within their description table.
Entity address is the starting address, a 16-bit value in the data part of the Modbus frame. As such its range goes from 0 to 65,535
In the traditional standard, numbers for those entities start with a digit, followed by a number of 4 digits in the range 1–9,999:

* coils numbers start with 0 and span from 00001 to 09999,
* discrete input numbers start with 1 and span from 10001 to 19999,
* input register numbers start with 3 and span from 30001 to 39999,
* holding register numbers start with 4 and span from 40001 to 49999.

This translates into addresses between 0 and 9,998 in data frames. For example, in order to read holding registers starting at number 40001, corresponding address in the data frame will be 0 with a function code of 3 (as seen above). For holding registers starting at number 40100, address will be 99. Etc.

This limits the number of addresses to 9,999 for each entity. A de facto referencing extends this to the maximum of 65,536. It simply consists of adding one digit to the previous list:

* coil numbers span from 000001 to 065536,
* discrete input numbers span from 100001 to 165536,
* input register numbers span from 300001 to 365536,
* holding register numbers span from 400001 to 465536.

When using the extended referencing, all number references must have exactly 6 digits. This avoids confusion between coils and other entities. For example, to know the difference between holding register #40001 and coil #40001, if coil #40001 is the target, it must appear as #040001.
