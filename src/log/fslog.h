#pragma once
#include <iostream>
#include <fstream>
static void LOG(const char* message) {
	std::cout << message << std::endl;
	std::fstream fs;
#ifndef FSLOG_NAME
#define FSLOG_NAME "fslog.txt"
#endif
	fs.open(FSLOG_NAME, std::fstream::in | std::fstream::out | std::fstream::app);
#ifdef FSLOG_PREFIX
	fs << FSLOG_PREFIX;
#endif
	fs << message;
	fs << "\n";
	fs.close();
}
//static void LOG(std::string message) {
//	LOG((char*)message.c_str());
//}
