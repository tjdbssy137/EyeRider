using UnityEngine;
using UniRx;

public partial class GameProfileContext
{
    //현재 레벨을 할지, 과거 레벨을 할지 잘 정해둬야할 듯. 지금 사실상 같은 의미?
    public int CurrentLevel  { get; set; }
    public int NextLevel { get; set; }
    public int CarId  { get; set; }
    public int Gold { get; set; }

}