#include <iostream>
#include <thread>
#include <Ultralight/Ultralight.h>
using namespace ultralight;
using namespace std;
class MyApp : public LoadListener {
	RefPtr<Renderer> renderer_;
	bool done_ = false;
public:
	RefPtr<View> view;
	MyApp() {
		Config config;
		config.device_scale_hint = 2.0;
		config.font_family_standard = "Arial";
		Platform::instance().set_config(config);
		renderer_ = Renderer::Create();
		view = renderer_->CreateView(32, 32, false); // https://github.com/ultralight-ux/Ultralight/issues/257#issuecomment-636330995
		view->Resize(32, 32);
		view->set_load_listener(this);
	}
	void SetURL(String url) {
		view->LoadURL(url);
	}
	~MyApp() {
		cout << "~MyApp" << endl;
		view = nullptr; // IT DONT WORK.
		cout << "view=nullptr" << endl;
		renderer_ = nullptr;
		cout << "renderer_=nullptr" << endl;
		cout << view << endl;
		cout << renderer_ << endl;
	}
	void Run() {
		std::cout << "Starting Run(), waiting for page to load..." << std::endl;
		while (!done_) {
			renderer_->Update();
		}
		std::cout << "Finished." << std::endl;
	}
	void OnFinishLoading(ultralight::View* caller) {
		renderer_->Render();
		view->bitmap()->WritePNG("result.png");
		std::cout << "Saved a render of our page to result.png." << std::endl;
		done_ = true;
	}
};
//int val = 0;
//bool done = false;

//void thr() {
//	while (true)
//	{
//		val++;
//		cout << val << endl;
//		done = true;
//	}
//}
int main() {
	//thread mthread(thr);
	//while (!done) {
	//	//cout << val << endl;
	//}
	//mthread.detach();
	MyApp app;
	app.SetURL("https://github.com");
	app.Run();
	cout << "END" << endl;
	return 0;
}