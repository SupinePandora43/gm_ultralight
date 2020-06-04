

#include <iostream> 
#include <thread> 
using namespace std;

int val = 0;
bool done = false;
void thr() {
	while (true)
	{
		val++;
		cout << val << endl;
		done = true;
	}
}
int main() {
	thread mthread(thr);
	while (!done) {
		//cout << val << endl;
	}
	mthread.detach();
	return 0;
}