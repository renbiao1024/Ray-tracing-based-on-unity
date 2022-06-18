using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitRecord
{
    //�������У��ӳ����ȣ����㣬���㷨��
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
        //ʵ�������ж����������û�и��������2�������ǻ���
        float discriminant = b * b - 4 * a * c;
        if (discriminant > 0)
        {
            //���벢������������Դ�ĵ�
            float temp = (-b - Mathf.Sqrt(discriminant)) / a * 0.5f;
            if (temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = ray.GetPoint(rec.t);
                rec.normal = (rec.p - center).normalized;
                return true;
            }
            //����ͼ���Զ������Դ�ĵ�
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
        foreach(var h in list)//����deÿһ�������ж��Ƿ�͹��ཻ
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