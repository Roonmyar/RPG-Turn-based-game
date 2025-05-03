using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;    // Радиус взрыва
    public float explosionForce = 700f;   // Сила отбрасывания
    public int damage = 50;              // Урон
    public GameObject explosionEffect;   // Префаб эффекта взрыва (частицы + свет)

    private bool _isExploded = false;

    // Вызывается при столкновении с другим объектом
    void OnCollisionEnter(Collision collision)
    {
        // Проверяем, если у объекта есть тег "Weapon" или он летит с большой скоростью
        if (collision.relativeVelocity.magnitude > 2f || collision.gameObject.CompareTag("Weapon"))
        {
            Explode();
        }
    }

    // Взорвать бочку
    public void Explode()
    {
        if (_isExploded) return;
        _isExploded = true;

        // 1. Создать эффект взрыва
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 2. Найти все объекты в радиусе взрыва
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Применить силу взрыва
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Нанести урон (если у объекта есть Health)
            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        // 3. Уничтожить бочку
        Destroy(gameObject);
    }
}