namespace H5.Http;
public interface IRequestHandler {
    void Handle(System.Net.HttpListenerContext context);
}
