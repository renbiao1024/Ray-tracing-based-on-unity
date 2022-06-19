using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Material
{
    public Color albedo;
    /// <summary>
    /// 材质发生的光线变化过程
    /// </summary>
    /// <param name="rayIn">进入的光线</param>
    /// <param name="record">存在的物体</param>
    /// <param name="attenuation">衰减</param>
    /// <param name="scattered">散射等光线变化</param>
    /// <returns>是否发生了光线变化</returns>
    public abstract bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered);
}

/// <summary>
/// 理想的漫反射模型
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
/// 理想的镜面反射模型
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

    public Vector3 reflect(Vector3 vin, Vector3 normal)
    {
        return vin - 2 * Vector3.Dot(vin, normal) * normal;
    }

    public bool refract(Vector3 vin, Vector3 normal, float ni_no, ref Vector3 refracted)
    {
        Vector3 uvin = vin.normalized;
        float dt = Vector3.Dot(uvin, normal);
        float discrimination = 1 - ni_no * ni_no * (1 - dt * dt);
        if (discrimination > 0)
        {
            refracted = ni_no * (uvin - normal * dt) - normal * Mathf.Sqrt(discrimination);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 菲涅尔反射
    /// </summary>
    /// <param name="cos"></param>
    /// <param name="ref_idx"></param>
    /// <returns></returns>
    public float schlick(float cos, float ref_idx)
    {
        float r0 = (1 - ref_idx) / (1 + ref_idx);
        r0 *= r0;
        return r0 + (1 - r0) * Mathf.Pow((1 - cos), 5);
    }
}

/// <summary>
/// 透明折射模型
/// </summary>
public class Dielectirc : Material
{
    private Metal _M = new Metal(Color.white);
    //相对空气的折射率
    float ref_idx;
    public Dielectirc(float ri) { ref_idx = ri;}
    public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
    {
        Vector3 outNormal;
        Vector3 reflected = _M.reflect(rayIn.direction, record.normal);//反射方向
        attenuation = Color.white;//完全透明物体不吸收光
        float ni_no = 1f;
        Vector3 refracted = Vector3.zero;

        float cos = 0;
        float reflect_prob = 0;

        //如果光从反方向进入介质，折射率和法线需要反转
        if (Vector3.Dot(rayIn.direction, record.normal) > 0)
        {
            outNormal = -record.normal;
            ni_no = ref_idx;
            cos = ni_no * Vector3.Dot(rayIn.normalDirection, record.normal);
        }
        else
        {
            outNormal = record.normal;
            ni_no = 1f / ref_idx;
            cos = -Vector3.Dot(rayIn.normalDirection, record.normal);
        }

        //如果没有发生折射就用反射
        if (_M.refract(rayIn.direction, outNormal, ni_no, ref refracted))
        {
            reflect_prob = _M.schlick(cos, ref_idx);
        }
        else
        {
            reflect_prob = 1;//全反射
        }

        if (Random.Range(0f, 1f) <= reflect_prob)
        {
            scattered =  new Ray(record.p, reflected);
        }
        else
        {
            scattered = new Ray(record.p, refracted);
        }
        return true;
    }

}
