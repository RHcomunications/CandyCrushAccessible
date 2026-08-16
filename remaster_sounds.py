import os
import shutil
import subprocess
from pathlib import Path

def remaster_audio_assets():
    base_dir = Path(__file__).parent.resolve()
    sounds_dir = base_dir / "sounds"
    legacy_dir = base_dir / "sounds_legacy"
    
    if not sounds_dir.exists():
        print(f"[ERROR] No se encontró la carpeta: {sounds_dir}")
        return
        
    # Crear carpeta de respaldo legacy
    legacy_dir.mkdir(parents=True, exist_ok=True)
    
    # Extensiones de entrada a procesar
    valid_exts = {".wav", ".ogg", ".mp3", ".m4a", ".flac"}
    files_to_process = [f for f in sounds_dir.iterdir() if f.is_file() and f.suffix.lower() in valid_exts]
    
    if not files_to_process:
        print("[!] No se encontraron archivos de audio para procesar en sounds/")
        return
        
    print(f"[*] Respaldando {len(files_to_process)} archivos en: {legacy_dir}...")
    for f in files_to_process:
        backup_file = legacy_dir / f.name
        if not backup_file.exists():
            shutil.copy2(f, backup_file)
            
    print("\n[*] Iniciando remasterización por lotes con FFmpeg...")
    print("    - Frecuencia: 44.1 kHz (CD Quality)")
    print("    - Normalización EBU R128: loudnorm=I=-14:TP=-1.0:LRA=11")
    print("    - Codec: Ogg Vorbis (Quality 5 / ~160-192 kbps)")
    print("-" * 65)
    
    success_count = 0
    failed_count = 0
    
    # Procesamos desde legacy_dir hacia sounds_dir
    legacy_files = [f for f in legacy_dir.iterdir() if f.is_file() and f.suffix.lower() in valid_exts]
    
    for idx, input_file in enumerate(legacy_files, 1):
        # Generar nombre normalizado de salida (.ogg)
        # Normalizamos también nombres con guiones y guiones bajos para máxima compatibilidad
        out_stem = input_file.stem
        output_ogg = sounds_dir / f"{out_stem}.ogg"
        
        # Comando FFmpeg con loudnorm y remuestreo a 44100
        cmd = [
            "ffmpeg", "-y", "-v", "error",
            "-i", str(input_file),
            "-ar", "44100",
            "-af", "loudnorm=I=-14:TP=-1.0:LRA=11",
            "-c:a", "libvorbis", "-q:a", "5",
            str(output_ogg)
        ]
        
        try:
            res = subprocess.run(cmd, capture_output=True, text=True)
            if res.returncode == 0:
                print(f"  [{idx:02d}/{len(legacy_files):02d}] OK -> {output_ogg.name}")
                success_count += 1
            else:
                print(f"  [{idx:02d}/{len(legacy_files):02d}] FALLO -> {input_file.name}: {res.stderr.strip()}")
                failed_count += 1
        except Exception as ex:
            print(f"  [{idx:02d}/{len(legacy_files):02d}] ERROR -> {input_file.name}: {ex}")
            failed_count += 1

    # Limpieza: eliminamos los archivos .wav antiguos de sounds/ ya que ahora tenemos los remasterizados .ogg
    for f in sounds_dir.iterdir():
        if f.is_file() and f.suffix.lower() == ".wav":
            ogg_equiv = sounds_dir / f"{f.stem}.ogg"
            if ogg_equiv.exists():
                f.unlink()

    print("-" * 65)
    print(f"[+] Proceso finalizado: {success_count} remasterizados con éxito, {failed_count} fallos.")
    print(f"[+] Los originales están seguros en: {legacy_dir}")

if __name__ == "__main__":
    remaster_audio_assets()
