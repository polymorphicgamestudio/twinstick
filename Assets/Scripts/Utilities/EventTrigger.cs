

namespace ShepProject
{

    public delegate void EventTrigger<T>(T data);

    public delegate void EventTrigger<T, T2>(T data, T2 data2);


    public delegate void EventTrigger();


}