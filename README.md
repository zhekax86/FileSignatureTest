# FileSignatureTest

A small test project made as a test task before interview for a software company which name I wouldn't like to disclose here.
This project still may be usefull as a example of code I can write.

## Test task description:

> Write a console program in C# for generation a signature of specified file. The signature is generated in the following order: 
> the file is split into blocks of specified length (except for the last one), for each block a value of SHA256 hash-function is calculated 
> and printed to a console along with the block number.
> 
> Program should be able to process files which size exceeds volume of RAM, and use resources of a multithreading system in the most efficient way.
> 
> While working with threads it is only possible to use standard classes from .Net library (excluding ThreadPool, BackgroundWorker, TPL)
> 
> A realization with Threads is expected. Path to the input file and the block size are specified in a command line.
> In case of error occurring while program execution an error and a stack trace should be printed to a console.
