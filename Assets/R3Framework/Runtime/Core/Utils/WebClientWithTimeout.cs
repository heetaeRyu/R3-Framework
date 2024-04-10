using System.Net;
using System;

namespace Netmarble.Core
{
	public class WebClientWithTimeout : WebClient
	{
		private int _connectTimeout;
		private int _timeout;
		private bool _isRange = false;
		private int _startRange = 0;
		private int _endRange = 0;
		private long _totalLength = 0L;

		public WebClientWithTimeout()
		{
			// NOTE : 타임아웃 초 설정이 외부 제어가 필요한가?
			_connectTimeout = 15 * 1000;
			_timeout = 15 * 1000;
		}

		public WebClientWithTimeout(int timeout)
		{
			_timeout = timeout;
			_connectTimeout = timeout;
		}

		public WebClientWithTimeout(int connectTimeout, int readWriteTimeout)
		{
			_connectTimeout = connectTimeout;
			_timeout = readWriteTimeout;
		}

		public void SetRange(int start)
		{
			_isRange = true;
			_startRange = start;
		}

		public void SetRange(int start, int end)
		{
			_isRange = true;
			_startRange = start;
			_endRange = end;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
			if (request != null)
			{
				request.Timeout = _connectTimeout;
				request.ReadWriteTimeout = _timeout;

				if (_isRange)
				{
					if (_startRange > 0 && _endRange > _startRange) request.AddRange(_startRange, _endRange);
					else if (_startRange > 0) request.AddRange(_startRange);
				}

				return request;
			}

			return null;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse wr = base.GetWebResponse(request);

			/*
					StringBuilder sb = new StringBuilder();
					foreach(string key in wr.Headers.AllKeys)
					{
						sb.AppendLine(string.Format("{0} = {1}", key, wr.Headers.Get(key)));
					}
					Debug.Log(sb.ToString());
			*/
			if (wr != null)
			{
				string contentRange = wr.Headers.Get("Content-Range");

				if (wr.Headers != null && contentRange != null &&
				    contentRange.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) > -1)
				{
					_totalLength =
						long.Parse(contentRange.Substring(
							contentRange.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1));
				}
				else
				{
					_totalLength = _startRange + wr.ContentLength;
				}
			}

			return wr;
		}

		public long GetTotalLength()
		{
			return _totalLength;
		}
	}
}