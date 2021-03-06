using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitRecord
{
    //描述命中，延长长度，交点，交点法线
    public float t;
    public Vector3 p;
    public Vector3 normal;
    public Material material; 
}

public abstract class Hitable
{
    public Material material;
    public abstract bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec);
}

public class Sphere : Hitable
{
    public Vector3 center;
    public float radius;

    public Sphere(Vector3 cen, float rad)
    {
        center = cen;
        radius = rad;
    }

    public Sphere(Vector3 cen, float rad, Material mat)
    {
        center = cen;
        radius = rad;
        material = mat;
    }

    public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
    {
        var oc = ray.original - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        //实际上是判断这个方程有没有根，如果有2个根就是击中
        float discriminant = b * b - 4 * a * c;
        if (discriminant > 0)
        {
            //带入并计算出最靠近射线源的点
            float temp = (-b - Mathf.Sqrt(discriminant)) / a * 0.5f;
            if (temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = ray.GetPoint(rec.t);
                rec.normal = (rec.p - center).normalized;
                return true;
            }
            //否则就计算远离射线源的点
            temp = (-b + Mathf.Sqrt(discriminant)) / a * 0.5f;
            if (temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = ray.GetPoint(rec.t);
                rec.normal = (rec.p - center).normalized;
                return true;
            }
        }
        return false;
    }
}

public class HitableList : Hitable
{
    public List<Hitable> list;
    public HitableList() { list = new List<Hitable>(); }

    public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
    {
        HitRecord tempRecord = new HitRecord();
        bool hitAnything = false;
        float closest = t_max;
        foreach(var h in list)//传入de每一个物体判断是否和光相交
        {
            if(h.Hit(ray,t_min,closest,ref tempRecord))
            {
                hitAnything = true;
                closest = tempRecord.t;
                rec = tempRecord;
                rec.material = h.material;
            }
        }
        return hitAnything;
    }
}