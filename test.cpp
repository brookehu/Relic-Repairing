#include <iostream>

using namespace std;

class stack{
private:
	Num()=delete;
public:
	int num;

	//Num(int n):num(n){
	//}
};

int main(){
	
	int v;
	float f = 3.1; 
	v=reinterpret_cast<int&>(f);
	cout << v;
	
	return 0;
} 
