using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Material
{
    public Color albedo;
    /// <summary>
    /// ���ʷ����Ĺ��߱仯����
    /// </summary>
    /// <param name="rayIn">����Ĺ���</param>
    /// <param name="record">���ڵ�����</param>
    /// <param name="attenuation">˥��</param>
    /// <param name="scattered">ɢ��ȹ��߱仯</param>
    /// <returns>�Ƿ����˹��߱仯</returns>
    public abstract bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered);
}

/// <summary>
/// �����������ģ��
/// </summary>
public class Lambertian : Material
{
    public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
    {
        Vector3 target = record.p + record.normal + GetRandomPointInUnitSphere();
        scattered = new Ray(record.p, target - record.p);
        attenuation = albedo;
        return true;
    }

    private Vector3 GetRandomPointInUnitSphere()
    {
        Vector3 p = 2f * new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) - Vector3.one;
        p = p.normalized * Random.Range(0f, 1f);
        return p;
    }
    public Lambertian(Color a) { albedo = a; }
}

/// <summary>
/// ����ľ��淴��ģ��
/// </summary>
public class Metal : Material
{
    public Metal(Color a) { albedo = a; }
    public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
    {
        Vector3 reflected = reflect(rayIn.normalDirection, record.normal);
        scattered = new Ray(record.p, reflected);
        attenuation = albedo;
        return true;// Vector3.Dot(scattered.direction, record.normal) > 0;
    }

    Vector3 reflect(Vector3 vin, Vector3 normal)
    {
        return vin - 2 * Vector3.Dot(vin, normal) * normal;
    }
}