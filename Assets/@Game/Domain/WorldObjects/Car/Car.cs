public class Car : BaseObject
{
    CarController _carController;
    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		return true;
	}
	
	public override bool OnSpawn()
    {
		if (false == base.OnSpawn())
        {
            return false;
        }

        _carController = this.gameObject.GetComponent<CarController>();
        _carController.OnSpawn();
        _carController.SetInfo(0);
        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        Contexts.InGame.Car = this;
        
    }
}