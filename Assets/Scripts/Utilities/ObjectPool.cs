using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyObjectPool : ObjectPool<EnemyPhysicsMethods>
{
    public EnemyObjectPool(EnemyPhysicsMethods prefab, ushort capacity, ushort initialCount)
        : base(prefab, capacity, initialCount)
    {


    }


    public override void ReturnObject(EnemyPhysicsMethods obj)
    {
        obj.gameObject.SetActive(false);

        base.ReturnObject(obj);


    }

    protected override EnemyPhysicsMethods InstantiatePrefab()
    {
        EnemyPhysicsMethods enemy = GameObject.Instantiate(prefab.gameObject).GetComponent<EnemyPhysicsMethods>();
        enemy.name = prefab.name + " " + pool.Count;

        return enemy;
    }
}



public class GameObjectPool : ObjectPool<GameObject>
{
    public GameObjectPool(GameObject prefab, ushort capacity, ushort initialCount)
        : base(prefab, capacity, initialCount)
    {


    }

    public override void ReturnObject(GameObject obj)
    {
        base.ReturnObject(obj);
        obj.SetActive(false);

        pool.Enqueue(obj);

    }


    protected override GameObject InstantiatePrefab()
    {

        return GameObject.Instantiate(prefab);

    }
}



public abstract class ObjectPool<T>
{

    protected Queue<T> pool;
    protected T prefab;

    public ObjectPool(T prefab, ushort capacity, ushort initialCount)
    {

        this.prefab = prefab;
        pool = new Queue<T>(capacity);

        for (ushort i = 0; i < initialCount; i++)
        {
            pool.Enqueue(InstantiatePrefab());

        }

    }

    protected abstract T InstantiatePrefab();


    public virtual void ReturnObject(T obj)
    {

        pool.Enqueue(obj);

    }

    public virtual T GetObject()
    {

        if (pool.Count == 0)
        {
            pool.Enqueue(InstantiatePrefab());
        }



        return pool.Dequeue();

    }




}
