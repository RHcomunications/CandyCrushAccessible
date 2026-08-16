import zipfile
from pathlib import Path
import shutil
import sys

def extract_candy_sounds(apk_path: str, output_dir: str):
    """
    Escanea un archivo APK y extrae todos los archivos de audio nativos
    directamente a un directorio de destino, aplanando la estructura de carpetas.
    """
    apk_file = Path(apk_path)
    out_path = Path(output_dir)
    
    if not apk_file.exists():
        print(f"[ERROR] No se encontró el archivo: {apk_file}")
        return
        
    out_path.mkdir(parents=True, exist_ok=True)
    
    # Extensiones de audio comunes en juegos móviles de 2012-2013
    valid_extensions = {'.mp3', '.ogg', '.wav', '.m4a', '.aac', '.flac'}
    extracted_count = 0
    
    print(f"[*] Diseccionando APK: {apk_file.name}...")
    
    with zipfile.ZipFile(apk_file, 'r') as apk:
        for file_info in apk.infolist():
            file_path = Path(file_info.filename)
            
            if file_path.suffix.lower() in valid_extensions:
                source_filename = file_path.name
                target_path = out_path / source_filename
                
                with apk.open(file_info.filename) as source, open(target_path, "wb") as target:
                    shutil.copyfileobj(source, target)
                    
                print(f"  -> Extraído: {source_filename} (desde {file_info.filename})")
                extracted_count += 1
                
    print(f"\n[+] ¡Misión cumplida! Se extrajeron {extracted_count} archivos de audio en: {out_path}")

if __name__ == "__main__":
    # Si se pasa un APK por argumento de línea de comandos, lo usamos:
    # Ejemplo: python extractor.py "C:\Users\artik\Downloads\candy_crush_2012.apk"
    if len(sys.argv) > 1:
        ruta_apk = sys.argv[1]
    else:
        ruta_apk = r"C:\Users\artik\Downloads\candy_crush_v1_2012.apk"
        
    ruta_destino = r"C:\Users\artik\Downloads\candy crush\sounds"

    extract_candy_sounds(ruta_apk, ruta_destino)
