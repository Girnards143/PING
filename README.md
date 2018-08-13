# PING

### TR

**PING** programı,yoğun bakım ünitelerinde bulunan monitör ve ventilatör cihazlarının IP adreslerine ping atarak başarısız olması halinde
2 saatte bir olacak şekilde mail ile bilgilendirme yapar. Ayrıca monitör ve ventilatör IP adreslerinin çalıştığı süre zarfındaki kesinti
ve deneme sayılarını istatistik tutar.

**PING_Service**, PING programının windows işletim sisteminde arkaplanda çalışmasını sağlayan servis programıdır. Program başlarken argüman
olarak .txt dosyalarının kaydedileceği dosya yolunu  ve programın çalışma aralığını gönderir.
