﻿using IORAMHelper;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace GenieLibrary
{
	/// <summary>
	/// Diese Klasse ist für das Laden, Verwalten und Schreiben einer DAT-Datei zuständig. Diese Datei muss bereits dekomprimiert sein, um diese lesen zu können.
	/// </summary>
	/// <remarks></remarks>
	public class GenieFile
	{
		#region Konstanten

		/// <summary>
		/// Marker für den neuen TechTree. Wird nach dem regulären TechTree in die DAT eingefügt, um Kompatibilität zu nicht gepatchten Spielversionen zu erhalten.
		/// Muss immer Länge 3 haben.
		/// </summary>
		private const string NEW_TECH_TREE_MARKER = "NTT";
		
		#endregion

		#region Variablen

		/// <summary>
		/// Der Dateiname der DAT-Datei mitsamt Pfad.
		/// </summary>
		/// <remarks></remarks>
		public string _filename = "";

		/// <summary>
		/// Enthält eine Instanz der RAMBuffer-Klasse und damit alle Daten.
		/// </summary>
		/// <remarks></remarks>
		private RAMBuffer _buffer = null;

		/// <summary>
		/// Gibt an, ob die neue TechTree-Datenstruktur benutzt wird.
		/// </summary>
		private bool _newTechTree = false;

		#region Datenelemente

		/// <summary>
		/// Die Terrain-Anzahl.
		/// </summary>
		public int TerrainCount;

		/// <summary>
		/// Pointer zu Terrain-Beschränkungen.
		/// </summary>
		public List<int> TerrainRestrictionPointers1;

		/// <summary>
		/// Pointer zu Terrain-Beschränkungen.
		/// </summary>
		public List<int> TerrainRestrictionPointers2;

		/// <summary>
		/// Die Terrain-Beschränkungen.
		/// </summary>
		public List<DataElements.TerrainRestriction> TerrainRestrictions;

		/// <summary>
		/// Die Spielerfarben.
		/// </summary>
		public List<DataElements.PlayerColor> PlayerColors;

		/// <summary>
		/// Die Sounds.
		/// </summary>
		public List<DataElements.Sound> Sounds;

		/// <summary>
		/// Grafik-Pointer.
		/// </summary>
		public List<int> GraphicPointers;

		/// <summary>
		/// Die Grafiken.
		/// </summary>
		public Dictionary<int, DataElements.Graphic> Graphics;

        /// <summary>
        /// Terrain-Daten.
        /// </summary>
        public DataElements.TerrainBlock TerrainBlock;

        /// <summary>
        /// RandomMap-Daten.
        /// </summary>
        public DataElements.RandomMaps RandomMaps;

        /// <summary>
        /// Die Technologie-Effekte.
        /// </summary>
        public List<DataElements.Techage> Techages;

		/// <summary>
		/// Die Einheiten-Header.
		/// </summary>
		public List<DataElements.UnitHeader> UnitHeaders;

		/// <summary>
		/// Die Kulturen.
		/// </summary>
		public List<DataElements.Civ> Civs;

		/// <summary>
		/// Die Technologien.
		/// </summary>
		public List<DataElements.Research> Researches;

		/// <summary>
		/// Der Technologie-Baum (neues Format).
		/// </summary>
		public DataElements.TechTreeNew TechTreeNew;

		/// <summary>
		/// Der Technologie-Baum (altes Format).
		/// </summary>
		public DataElements.TechTree TechTree;

		/// <summary>
		/// Unbekannte Daten bezüglich des Technologie-Baums (altes Format).
		/// </summary>
		public List<int> TechTreeUnknown;

		#endregion Datenelemente

		#endregion Variablen

		#region Funktionen

		/// <summary>
		/// Erstellt eine neue Instanz von GenieFile und lädt alle Daten.
		/// </summary>
		/// <param name="filename">Der Pfad und Dateiname der zu ladenden (unkomprimierten) DAT-Datei.</param>
		/// <remarks></remarks>
		public GenieFile(string filename)
		{
			// Speichern des Dateinamens
			_filename = filename;

			// Sicherstellen, dass die Datei existiert
			if(!File.Exists(filename))
			{
				MessageBox.Show("Fehler: Die DAT-Datei wurde nicht gefunden!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Erstellen des Puffer-Objekts und Laden der Daten
			_buffer = new RAMBuffer(_filename);

			// Laden der Daten
			ReadData();
		}

		/// <summary>
		/// Erstellt eine neue Instanz von GenieFile und lädt alle Daten aus dem übergebenen Puffer.
		/// </summary>
		/// <param name="buffer">Der Puffer mit den zu ladenden Daten.</param>
		/// <remarks></remarks>
		public GenieFile(RAMBuffer buffer)
		{
			// Puffer speichern
			_buffer = buffer;

			// Laden der Daten
			ReadData();
		}

		/// <summary>
		/// Lädt alle Daten aus dem internen Puffer in die jeweiligen Variablen.
		/// </summary>
		/// <remarks></remarks>
		private void ReadData()
		{
			// Dateiversion lesen
			string version = _buffer.ReadString(8);
			if(version != "VER 5.7\0")
				throw new InvalidDataException("Invalid file format or wrong version.");

			// Anzahlen lesen
			ushort terrainRestrictionCount = _buffer.ReadUShort();
			ushort terrainCount = _buffer.ReadUShort();

			// Terrain-Pointer lesen
			TerrainRestrictionPointers1 = new List<int>(terrainRestrictionCount);
			for(int i = 0; i < terrainRestrictionCount; ++i)
				TerrainRestrictionPointers1.Add(_buffer.ReadInteger());
			TerrainRestrictionPointers2 = new List<int>(terrainRestrictionCount);
			for(int i = 0; i < terrainRestrictionCount; ++i)
				TerrainRestrictionPointers2.Add(_buffer.ReadInteger());

			// Terrain-Beschränkungen lesen
			TerrainRestrictions = new List<DataElements.TerrainRestriction>(terrainRestrictionCount);
			DataElements.TerrainRestriction.TerrainCount = terrainCount;
			for(int i = 0; i < terrainRestrictionCount; ++i)
				TerrainRestrictions.Add(new DataElements.TerrainRestriction().ReadData(_buffer));

			// Anzahl lesen
			ushort playerColorCount = _buffer.ReadUShort();

			// Spielerfarben lesen
			PlayerColors = new List<DataElements.PlayerColor>(playerColorCount);
			for(int i = 0; i < playerColorCount; ++i)
				PlayerColors.Add(new DataElements.PlayerColor().ReadData(_buffer));

			// Anzahl lesen
			ushort soundCount = _buffer.ReadUShort();

			// Sounds lesen
			Sounds = new List<DataElements.Sound>(soundCount);
			for(int i = 0; i < soundCount; ++i)
				Sounds.Add(new DataElements.Sound().ReadData(_buffer));

			// Anzahl lesen
			int graphicCount = _buffer.ReadUShort();

			// Grafik-Pointer lesen
			GraphicPointers = new List<int>(graphicCount);
			for(int i = 0; i < graphicCount; ++i)
				GraphicPointers.Add(_buffer.ReadInteger());

			// Grafiken lesen
			Graphics = new Dictionary<int, DataElements.Graphic>(graphicCount);
			for(int p = 0; p < GraphicPointers.Count; ++p)
				if(GraphicPointers[p] != 0)
					Graphics.Add(p, new DataElements.Graphic().ReadData(_buffer));

            // Terrain-Daten lesen
            TerrainBlock = new DataElements.TerrainBlock().ReadData(_buffer);

            // RandomMap-Daten lese
            RandomMaps = new DataElements.RandomMaps().ReadData(_buffer);

			// Anzahl lesen
			int techageCount = _buffer.ReadInteger();

			// Technologie-Effekte lesen
			Techages = new List<DataElements.Techage>(techageCount);
			for(int i = 0; i < techageCount; ++i)
				Techages.Add(new DataElements.Techage().ReadData(_buffer));

			// Anzahl lesen
			int unitCount = _buffer.ReadInteger();

			// Einheiten-Header lesen
			UnitHeaders = new List<DataElements.UnitHeader>(unitCount);
			for(int i = 0; i < unitCount; ++i)
				UnitHeaders.Add(new DataElements.UnitHeader().ReadData(_buffer));

			// Anzahl lesen
			int civCount = _buffer.ReadUShort();

			// Kulturen lesen
			Civs = new List<DataElements.Civ>(civCount);
			for(int i = 0; i < civCount; ++i)
				Civs.Add(new DataElements.Civ().ReadData(_buffer));

			// Anzahl lesen
			int researchCount = _buffer.ReadUShort();

			// Technologien lesen
			Researches = new List<DataElements.Research>(researchCount);
			for(int i = 0; i < researchCount; ++i)
				Researches.Add(new DataElements.Research().ReadData(_buffer));

			// Unbekannte Technologiebaum-Daten lesen
			TechTreeUnknown = new List<int>(7);
			for(int i = 0; i < 7; ++i)
				TechTreeUnknown.Add(_buffer.ReadInteger());

			// TechTree lesen
			TechTree = new DataElements.TechTree().ReadData(_buffer);

			// Auf neuen TechTree prüfen
			if(_buffer.Length - _buffer.Position > 4)
			{
				// Marker einlesen
				string newTechTreeMarker = _buffer.ReadString(3);
				if(newTechTreeMarker == NEW_TECH_TREE_MARKER)
					_newTechTree = true;
			}
			if(_newTechTree)
				TechTreeNew = new DataElements.TechTreeNew().ReadData(_buffer);

			// Puffer leeren, um Speicher zu sparen
			_buffer.Clear();
		}

		/// <summary>
		/// Geerbt. Liest Daten ab der gegebenen Position im Puffer.
		/// </summary>
		/// <param name="buffer">Der Puffer, aus dem die Daten gelesen werden sollen.</param>
		public void ReadData(RAMBuffer buffer)
		{
			// Puffer als internen Puffer speichern und Daten laden
			_buffer = buffer;
			ReadData();
		}

		/// <summary>
		/// Schreibt die enthaltenen Daten in das interne RAMBuffer-Objekt.
		/// </summary>
		/// <remarks></remarks>
		private void WriteData()
		{
			// Puffer initialisieren
			if(_buffer == null)
				_buffer = new RAMBuffer();
			else if(_buffer.Length != 0)
				_buffer.Clear();

			// Dateiversion schreiben
			_buffer.WriteString("VER 5.7", 8);

			// Anzahlen schreiben
			IGenieDataElement.AssertTrue(TerrainRestrictionPointers1.Count == TerrainRestrictionPointers2.Count);
			_buffer.WriteUShort((ushort)TerrainRestrictionPointers1.Count);
			_buffer.WriteUShort((ushort)DataElements.TerrainRestriction.TerrainCount);

			// Terrain-Pointer schreiben
			TerrainRestrictionPointers1.ForEach(e => _buffer.WriteInteger(e));
			TerrainRestrictionPointers2.ForEach(e => _buffer.WriteInteger(e));

			// Terrain-Beschränkungen schreiben
			TerrainRestrictions.ForEach(e => e.WriteData(_buffer));

			// Anzahl schreiben
			_buffer.WriteUShort((ushort)PlayerColors.Count);

			// Spielerfarben schreiben
			PlayerColors.ForEach(e => e.WriteData(_buffer));

			// Anzahl schreiben
			_buffer.WriteUShort((ushort)Sounds.Count);

			// Sounds schreiben
			Sounds.ForEach(e => e.WriteData(_buffer));

			// Anzahl schreiben
			_buffer.WriteUShort((ushort)GraphicPointers.Count);

			// Grafik-Pointer schreiben
			GraphicPointers.ForEach(e => _buffer.WriteInteger(e));

			// Grafiken schreiben Sicherstellen, dass genau jede definierte Grafik einen entsprechenden Pointer hat; hier nur über die Listenlänge, sollte aber die meisten auftretenden Fehler abdecken
			IGenieDataElement.AssertListLength(Graphics, GraphicPointers.Count(p => p != 0));
			foreach(var e in Graphics)
				e.Value.WriteData(_buffer);

            // Terrain-Daten schreiben
            TerrainBlock.WriteData(_buffer);

            // RandomMap-Daten schreiben
            RandomMaps.WriteData(_buffer);

            // Anzahl schreiben
            _buffer.WriteInteger(Techages.Count);

			// Technologie-Effekte schreiben
			Techages.ForEach(e => e.WriteData(_buffer));

			// Anzahl schreiben
			_buffer.WriteInteger(UnitHeaders.Count);

			// Einheiten-Header schreiben
			UnitHeaders.ForEach(e => e.WriteData(_buffer));

			// Anzahl schreiben
			_buffer.WriteUShort((ushort)Civs.Count);

			// Kulturen schreiben
			Civs.ForEach(e => e.WriteData(_buffer));

			// Anzahl schreiben
			_buffer.WriteUShort((ushort)Researches.Count);

			// Technologien schreiben
			Researches.ForEach(e => e.WriteData(_buffer));

			// Unbekannte Technologiebaum-Daten schreiben
			IGenieDataElement.AssertListLength(TechTreeUnknown, 7);
			TechTreeUnknown.ForEach(e => _buffer.WriteInteger(e));

			// Technologiebaum schreiben
			TechTree.WriteData(_buffer);
			if(_newTechTree)
			{
				// Marker und neuen TechTree schreiben
				_buffer.WriteString(NEW_TECH_TREE_MARKER);
				TechTreeNew.WriteData(_buffer);
			}

			// Fertig
		}

		/// <summary>
		/// Schreibt die enthaltenen Daten in eine neue DAT-Datei.
		/// </summary>
		/// <param name="destFile">Die Datei, in die die Daten geschrieben werden sollen.</param>
		/// <remarks></remarks>
		public void WriteData(string destFile)
		{
			// Daten in Puffer schreiben
			WriteData();

			// Puffer in Datei schreiben
			_buffer.Save(destFile);
		}

		/// <summary>
		/// Schreibt die enthaltenen Daten in den übergebenen Stream.
		/// </summary>
		/// <param name="destFile">Der Stream, in den die Daten geschrieben werden sollen.</param>
		/// <remarks></remarks>
		public void WriteData(Stream stream)
		{
			// Daten in Puffer schreiben
			WriteData();

			// Puffer in Stream schreiben
			_buffer.ToMemoryStream().CopyTo(stream);
		}

		/// <summary>
		/// Geerbt. Schreibt die enthaltenen Daten an die gegebene Position im Puffer.
		/// </summary>
		/// <param name="buffer">Der Puffer, in den die Daten geschrieben werden sollen.</param>
		public void WriteData(RAMBuffer buffer)
		{
			// Puffer als internen Puffer speichern und Daten schreiben
			_buffer = buffer;
			WriteData();
		}

		#endregion Funktionen

		#region Statische Funktionen

		/// <summary>
		/// Komprimiert die gegebenen DAT-Daten (zlib-Kompression).
		/// </summary>
		/// <param name="dat">Die zu komprimierenden Daten.</param>
		/// <returns></returns>
		public static RAMBuffer CompressData(RAMBuffer dat)
		{
			// Ausgabe-Stream erstellen
			MemoryStream output = new MemoryStream();

			// Daten in Memory-Stream schreiben
			using(MemoryStream input = dat.ToMemoryStream())
			{
				// Kompressions-Stream erstellen
				using(DeflateStream compressor = new DeflateStream(output, CompressionMode.Compress))
				{
					// (De-)Komprimieren
					input.CopyTo(compressor);
					input.Close();
				}
			}

			// Ergebnis in Puffer schreiben
			return new RAMBuffer(output.ToArray());
		}

		/// <summary>
		/// Dekomprimiert die gegebenen DAT-Daten (zlib-Kompression).
		/// </summary>
		/// <param name="dat">Die zu dekomprimierenden Daten.</param>
		/// <returns></returns>
		public static RAMBuffer DecompressData(RAMBuffer dat)
		{
			// Ausgabe-Stream erstellen
			MemoryStream output = new MemoryStream();

			// Daten in Memory-Stream schreiben
			using(MemoryStream input = dat.ToMemoryStream())
			{
				// Kompressions-Stream erstellen
				using(DeflateStream decompressor = new DeflateStream(input, CompressionMode.Decompress))
				{
					// (De-)Komprimieren
					decompressor.CopyTo(output);
					decompressor.Close();
				}
			}

			// Ergebnis in Puffer schreiben
			return new RAMBuffer(output.ToArray());
		}

		#endregion Statische Funktionen

		#region Eigenschaften

		/// <summary>
		/// Gibt an, ob beim Lese das neue TechTree-Format erkannt wurde bzw. ob das neue TechTree-Format geschrieben werden soll.
		/// Das neue Format kann auch dann geschrieben werden, wenn vorher das alte gelesen wurde (und andersherum), sofern alle zugehörigen Objekte entsprechend initialisiert sind.
		/// </summary>
		public bool NewTechTree
		{
			get { return _newTechTree; }
			set { _newTechTree = value; }
		}

		#endregion Eigenschaften
	}
}