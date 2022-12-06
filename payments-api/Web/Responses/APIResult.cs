using PaymentsAPI.Web.Responses;

namespace PaymentsAPI.Web
{

    public class APIResult<TObject>
    {
        public TObject? Value { get; }
        public APIError? APIError { get; }

        public APIResult()
        {
        }
        protected APIResult(TObject _o)
        {
            Value = _o;
        }
        protected APIResult(APIError error)
        {
            APIError = error;
        }

        public static APIResult<TObject> Success(TObject p)
        {
            return new APIResult<TObject>(p);
        }

        public static APIResult<TObject> Error(APIError e)
        {
            return new APIResult<TObject>(e);
        }
    }
}