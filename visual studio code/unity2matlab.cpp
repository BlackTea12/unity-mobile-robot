#pragma comment(lib, "ws2_32")
#pragma warning(disable:4996)

#include <iostream>
#include <winsock2.h>
#include <thread>
#include <fstream>
#include <windows.h>
#include <string>

using namespace std;

#define PACKET_SIZE 1024
#define SLEEPTIME3 120
#define SLEEPTIME6 60
#define SLEEPTIME9 40

SOCKET skt;

// reading file
string file_name = "vehicle_status.txt";
ofstream status;

void proc_recv() {
	char buffer[PACKET_SIZE] = {}; //char 생성
	string cmd; //string 생성
	while (!WSAGetLastError()) {
		ZeroMemory(&buffer, PACKET_SIZE); //buffer 비우기
		recv(skt, buffer, PACKET_SIZE, 0); //데이터받아오기
		cmd = buffer; //buffer의값이 cmd에 들어갑니다
		if (cmd == "hi") break; //cmd의값이 "exit"일경우 데이터받아오기'만' 종료
		cout << "받은 메세지: " << buffer << endl;
		status << buffer;
	}
}

int main() {
	// communication
	WSADATA wsa;

	// open reading file
	status.open(file_name);

	if (!status.is_open())
	{
		cout << "file unable to open...\n";
		return 1;
	}
	cout << "data file open...\n";

	// connecting socket, if error returns 1
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		return 1;
	}

	skt = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);

	SOCKADDR_IN addr = {};
	addr.sin_family = AF_INET;
	addr.sin_port = htons(4444);
	addr.sin_addr.s_addr = inet_addr("127.0.0.1");

	// wait until connection with server
	while (1) {
		if (!connect(skt, (SOCKADDR*)&addr, sizeof(addr))) {
			cout << "connected\n";
			break;
		}
	}
	// this is from server
	thread proc1(proc_recv);
	char msg[PACKET_SIZE] = { 0 };

	// waiting for child thread server to end
	proc1.join();

	// write file close
	status.close();

	cout << "writing ends...\n";
	// communication socket close
	closesocket(skt);
	WSACleanup();
}