#ifndef CINEMATIC_REEF_COMMON
#define CINEMATIC_REEF_COMMON
float ReefCaustics(float2 p, float time)
{
    float2 q = p * 1.1;
    q += float2(sin(q.y * .67 + time * .27), cos(q.x * .51 - time * .23)) * .72;
    float a = sin(q.x * 2.1 + sin(q.y * 1.6 + time * .34));
    float b = sin(q.y * 2.3 + sin(q.x * 1.45 - time * .3));
    float ridge = 1.0 - abs(a + b) * .5;
    return pow(saturate(ridge), 18.0);
}
#endif
