namespace H5.Lib.Utils;
public static class AsyncUtils {
	public static T Sync<T>(this Task<T> task) {
		task.Wait();
		return task.Result;
	}
}
