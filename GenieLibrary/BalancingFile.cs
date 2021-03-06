﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GenieLibrary.DataElements;

namespace GenieLibrary
{
	/// <summary>
	/// Contains the balancing data diffs.
	/// The save/load mechanism relies on unchanged unit types between saving and loading, else there may be load errors.
	/// </summary>
	public class BalancingFile : INotifyPropertyChanged
	{
		#region Constants

		/// <summary>
		/// The version of the balancing file format.
		/// </summary>
		private const int Version = 2;

		#endregion

		#region Variables

		/// <summary>
		/// All modifiable units, indexed by their IDs.
		/// </summary>
		private Dictionary<short, UnitEntry> _unitEntries;

		/// <summary>
		/// All modifiable researches, indexed by their IDs.
		/// </summary>
		private Dictionary<short, ResearchEntry> _researchEntries;

		/// <summary>
		/// ID mapping file for internal use.
		/// </summary>
		private MappingFile _mappingFile;

		#endregion

		#region Properties

		/// <summary>
		/// Returns the list of all modifiable units, indexed by their IDs.
		/// </summary>
		public Dictionary<short, UnitEntry> UnitEntries
		{
			get { return _unitEntries; }
			private set
			{
				_unitEntries = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnitEntries)));
			}
		}

		/// <summary>
		/// Returns the list of all modifiable researches, indexed by their IDs.
		/// </summary>
		public Dictionary<short, ResearchEntry> ResearchEntries
		{
			get { return _researchEntries; }
			private set
			{
				_researchEntries = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResearchEntries)));
			}
		}

		#endregion

		#region Functions

		/// <summary>
		/// Creates a new empty balancing data object.
		/// </summary>
		/// <param name="genieFile">The genie file containing the base values the diffs are build upon.</param>
		/// <param name="languageFiles">The language DLL files, sorted by priority, descending. Used for proper name retrieval.</param>
		/// <param name="mappingFile">Optional. ID mapping file.</param>
		public BalancingFile(GenieLibrary.GenieFile genieFile, string[] languageFiles, MappingFile mappingFile = null)
		{
			// Remember mapping file
			_mappingFile = mappingFile;

			// Load language files for proper name display
			GenieLibrary.LanguageFileWrapper langFileWrapper = new GenieLibrary.LanguageFileWrapper(languageFiles);

			// Initialize unit list with base values
			Dictionary<short, UnitEntry> unitEntries = new Dictionary<short, UnitEntry>();
			foreach(Civ c in genieFile.Civs)
			{
				// Check for units not contained in the unit entry list
				foreach(KeyValuePair<int, Civ.Unit> unitData in c.Units)
				{
					// Unit already contained in unit entry list?
					if(unitEntries.ContainsKey((short)unitData.Key))
						continue;

					// Show only projectiles, living units and buildings
					if(unitData.Value.Type < Civ.Unit.UnitType.Projectile)
						continue;

					// Create entry
					UnitEntry ue = new UnitEntry();
					ue.DisplayName = langFileWrapper.GetString(unitData.Value.LanguageDLLName);
					if(string.IsNullOrEmpty(ue.DisplayName) || unitData.Value.Type == Civ.Unit.UnitType.Projectile)
						ue.DisplayName = unitData.Value.Name1.TrimEnd('\0');

					// Get members
					ue.HitPoints = new DiffElement<short>(ue, unitData.Value.HitPoints);
					ue.Speed = new DiffElement<float>(ue, unitData.Value.Speed);
					if(unitData.Value.DeadFish != null)
						ue.RotationSpeed = new DiffElement<float>(ue, unitData.Value.DeadFish.RotationSpeed);
					ue.LineOfSight = new DiffElement<float>(ue, unitData.Value.LineOfSight);
					if(unitData.Value.Bird != null)
						ue.SearchRadius = new DiffElement<float>(ue, unitData.Value.Bird.SearchRadius);
					if(unitData.Value.Type50 != null)
						ue.MinRange = new DiffElement<float>(ue, unitData.Value.Type50.MinRange);
					if(unitData.Value.Type50 != null)
						ue.MaxRange = new DiffElement<float>(ue, unitData.Value.Type50.MaxRange);
					if(unitData.Value.Type50 != null)
						ue.DisplayedRange = new DiffElement<float>(ue, unitData.Value.Type50.DisplayedRange);
					if(unitData.Value.Type50 != null)
						ue.ReloadTime = new DiffElement<float>(ue, unitData.Value.Type50.ReloadTime);
					if(unitData.Value.Type50 != null)
						ue.DisplayedReloadTime = new DiffElement<float>(ue, unitData.Value.Type50.DisplayedReloadTime);
					if(unitData.Value.Type50 != null)
						ue.BlastRadius = new DiffElement<float>(ue, unitData.Value.Type50.BlastRadius);
					if(unitData.Value.Type50 != null)
						ue.Attacks = new AttackArmorEntryListDiffElement
						(
							ue,
							new List<AttackArmorEntry>
							(
								unitData.Value.Type50.Attacks.Select(at => new AttackArmorEntry(at.Key, at.Value))
							)
						);
					if(unitData.Value.Type50 != null)
						ue.DisplayedAttack = new DiffElement<short>(ue, unitData.Value.Type50.DisplayedAttack);
					if(unitData.Value.Creatable != null)
						ue.ProjectileCount = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileCount);
					if(unitData.Value.Creatable != null)
						ue.ProjectileCountOnFullGarrison = new DiffElement<byte>(ue, unitData.Value.Creatable.ProjectileCountOnFullGarrison);
					if(unitData.Value.Type50 != null)
						ue.ProjectileFrameDelay = new DiffElement<short>(ue, unitData.Value.Type50.ProjectileFrameDelay);
					if(unitData.Value.Type50 != null)
						ue.ProjectileAccuracyPercent = new DiffElement<short>(ue, unitData.Value.Type50.ProjectileAccuracyPercent);
					if(unitData.Value.Type50 != null)
						ue.ProjectileDispersion = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileDispersion);
					if(unitData.Value.Type50 != null)
						ue.ProjectileGraphicDisplacementX = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileGraphicDisplacement[0]);
					if(unitData.Value.Type50 != null)
						ue.ProjectileGraphicDisplacementY = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileGraphicDisplacement[1]);
					if(unitData.Value.Type50 != null)
						ue.ProjectileGraphicDisplacementZ = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileGraphicDisplacement[2]);
					if(unitData.Value.Creatable != null)
						ue.ProjectileSpawningAreaWidth = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileSpawningAreaWidth);
					if(unitData.Value.Creatable != null)
						ue.ProjectileSpawningAreaHeight = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileSpawningAreaHeight);
					if(unitData.Value.Creatable != null)
						ue.ProjectileSpawningAreaRandomness = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileSpawningAreaRandomness);
					if(unitData.Value.Type50 != null)
						ue.Armors = new AttackArmorEntryListDiffElement
						(
							ue,
							new List<AttackArmorEntry>
							(
								unitData.Value.Type50.Armors.Select(at => new AttackArmorEntry(at.Key, at.Value))
							)
						);
					if(unitData.Value.Type50 != null)
						ue.DisplayedMeleeArmor = new DiffElement<short>(ue, unitData.Value.Type50.DisplayedMeleeArmor);
					if(unitData.Value.Creatable != null)
						ue.DisplayedPierceArmor = new DiffElement<short>(ue, unitData.Value.Creatable.DisplayedPierceArmor);
					ue.GarrisonCapacity = new DiffElement<byte>(ue, unitData.Value.GarrisonCapacity);
					if(unitData.Value.Building != null)
						ue.GarrisonHealRateFactor = new DiffElement<float>(ue, unitData.Value.Building.GarrisonHealRateFactor);
					if(unitData.Value.Creatable != null)
						ue.TrainTime = new DiffElement<short>(ue, unitData.Value.Creatable.TrainTime);
					if(unitData.Value.Creatable != null)
						ue.Cost1 = new ResourceCostEntryDiffElement(ue, new ResourceCostEntry
						(
							unitData.Value.Creatable.ResourceCosts[0].Type,
							unitData.Value.Creatable.ResourceCosts[0].Amount,
							(byte)unitData.Value.Creatable.ResourceCosts[0].Mode
						));
					if(unitData.Value.Creatable != null)
						ue.Cost2 = new ResourceCostEntryDiffElement(ue, new ResourceCostEntry
						(
							unitData.Value.Creatable.ResourceCosts[1].Type,
							unitData.Value.Creatable.ResourceCosts[1].Amount,
							(byte)unitData.Value.Creatable.ResourceCosts[1].Mode
						));
					if(unitData.Value.Creatable != null)
						ue.Cost3 = new ResourceCostEntryDiffElement(ue, new ResourceCostEntry
						(
							unitData.Value.Creatable.ResourceCosts[2].Type,
							unitData.Value.Creatable.ResourceCosts[2].Amount,
							(byte)unitData.Value.Creatable.ResourceCosts[2].Mode
						));

					// Assign name of secondary projectile, if defined
					if(unitData.Value.Creatable != null && c.Units.Keys.Contains(unitData.Value.Creatable.AlternativeProjectileUnit))
						ue.SecondaryProjectileName = $"[{unitData.Value.Creatable.AlternativeProjectileUnit}] { c.Units[unitData.Value.Creatable.AlternativeProjectileUnit].Name1.TrimEnd('\0')}";

					// Save unit entry
					unitEntries[(short)unitData.Key] = ue;
				}
			}

			// Sort and save unit entry list
			UnitEntries = unitEntries.OrderBy(ue => ue.Value.DisplayName).ToDictionary(ue => ue.Key, ue => ue.Value);

			// Initialize research list with base values
			Dictionary<short, ResearchEntry> researchEntries = new Dictionary<short, ResearchEntry>();
			for(int rId = 0; rId < genieFile.Researches.Count; ++rId)
			{
				// Get research data
				Research researchData = genieFile.Researches[rId];

				// Create entry
				ResearchEntry re = new ResearchEntry();
				re.DisplayName = langFileWrapper.GetString(researchData.LanguageDLLName1);
				if(string.IsNullOrEmpty(re.DisplayName))
					re.DisplayName = researchData.Name.TrimEnd('\0');
				if(string.IsNullOrWhiteSpace(re.DisplayName))
					continue; // Skip empty researches

				// Get members
				re.ResearchTime = new DiffElement<short>(re, researchData.ResearchTime);
				re.Cost1 = new ResourceCostEntryDiffElement(re, new ResourceCostEntry
				(
					researchData.ResourceCosts[0].Type,
					researchData.ResourceCosts[0].Amount,
					researchData.ResourceCosts[0].Mode
				));
				re.Cost2 = new ResourceCostEntryDiffElement(re, new ResourceCostEntry
				(
					researchData.ResourceCosts[1].Type,
					researchData.ResourceCosts[1].Amount,
					researchData.ResourceCosts[1].Mode
				));
				re.Cost3 = new ResourceCostEntryDiffElement(re, new ResourceCostEntry
				(
					researchData.ResourceCosts[2].Type,
					researchData.ResourceCosts[2].Amount,
					researchData.ResourceCosts[2].Mode
				));

				// Save research entry
				researchEntries[(short)rId] = re;
			}

			// Sort and save research entry list
			ResearchEntries = researchEntries.OrderBy(re => re.Value.DisplayName).ToDictionary(re => re.Key, re => re.Value);
		}

		/// <summary>
		/// Loads the balancing file at the given path.
		/// </summary>
		/// <param name="genieFile">The genie file containing the base values the diffs are build upon.</param>
		/// <param name="path">The path to the balancing file.</param>
		/// <param name="languageFiles">The language DLL files, sorted by priority, descending. Used for proper name retrieval.</param>
		/// <param name="mappingFile">Optional. ID mapping file.</param>
		public BalancingFile(GenieLibrary.GenieFile genieFile, string path, string[] languageFiles, MappingFile mappingFile = null)
			: this(genieFile, languageFiles)
		{
			// Load file into buffer
			IORAMHelper.RAMBuffer buffer = new IORAMHelper.RAMBuffer(path);

			// Check version
			int version = buffer.ReadInteger();
			if(version > Version)
				throw new ArgumentException("The given file was created with a newer version of this program, please consider updating.");

			// Check for embedded mapping file, and create ID conversion functions if necessary
			Func<short, short> ConvertUnitId = null;
			Func<short, short> ConvertResearchId = null;
			MappingFile embeddedMapping = null;
			if(buffer.ReadByte() == 1)
			{
				// Read embedded file
				embeddedMapping = new MappingFile(buffer);
				if(mappingFile == null || mappingFile.Hash.SequenceEqual(embeddedMapping.Hash))
				{
					// Use embedded file, no conversion required
					_mappingFile = embeddedMapping;
					ConvertUnitId = (id) => id;
					ConvertResearchId = (id) => id;
				}
				else
				{
					// Use new mapping file, create conversion functions (old DAT ID -> Editor ID -> new DAT ID)
					_mappingFile = mappingFile;
					ConvertUnitId = (id) => _mappingFile.UnitMapping.FirstOrDefault(m => m.Value == embeddedMapping.UnitMapping[id]).Key;
					ConvertResearchId = (id) => _mappingFile.ResearchMapping.FirstOrDefault(m => m.Value == embeddedMapping.ResearchMapping[id]).Key;
				}
			}
			else if(mappingFile != null)
				throw new ArgumentException("A mapping cannot be added to an existing file. Create a new balancing file instead.");

			// Read unit entries
			int unitEntryCount = buffer.ReadInteger();
			for(int i = 0; i < unitEntryCount; ++i)
			{
				// Read entry and merge with existing entry
				short unitId = ConvertUnitId(buffer.ReadShort());
				UnitEntries[unitId].Read(buffer);
			}

			// Read research entries
			int researchEntryCount = buffer.ReadInteger();
			for(int i = 0; i < researchEntryCount; ++i)
			{
				// Read entry and merge with existing entry
				short researchId = ConvertResearchId(buffer.ReadShort());
				ResearchEntries[researchId].Read(buffer);
			}
		}

		/// <summary>
		/// Saves the balancing file at the given path.
		/// </summary>
		/// <param name="path">The path where the balancing file shall be saved.</param>
		public void Save(string path)
		{
			// Create buffer
			IORAMHelper.RAMBuffer buffer = new IORAMHelper.RAMBuffer();

			// Write version
			buffer.WriteInteger(Version);

			// Write mapping data, if existing
			if(_mappingFile == null)
				buffer.WriteByte(0);
			else
			{
				buffer.WriteByte(1);
				_mappingFile.WriteData(buffer);
			}

			// Run through unit list and save unit entries
			int unitEntryCount = 0;
			int unitEntryCountOffset = buffer.Position;
			buffer.WriteInteger(unitEntryCount); // Placeholder
			foreach(KeyValuePair<short, UnitEntry> ue in UnitEntries)
			{
				// Are there any changes? => Omit units with no modifications
				if(ue.Value.ModifiedFieldsCount == 0)
					continue;

				// Save ID
				buffer.WriteShort(ue.Key);

				// Save entry data
				ue.Value.Save(buffer);
				++unitEntryCount;
			}

			// Run through research list and save research entries
			int researchEntryCount = 0;
			int researchEntryCountOffset = buffer.Position;
			buffer.WriteInteger(researchEntryCount); // Placeholder
			foreach(KeyValuePair<short, ResearchEntry> re in ResearchEntries)
			{
				// Are there any changes? => Omit researches with no modifications
				if(re.Value.ModifiedFieldsCount == 0)
					continue;

				// Save ID
				buffer.WriteShort(re.Key);

				// Save entry data
				re.Value.Save(buffer);
				++researchEntryCount;
			}

			// Write unit entry count
			buffer.Position = unitEntryCountOffset;
			buffer.WriteInteger(unitEntryCount);

			// Write research entry count
			buffer.Position = researchEntryCountOffset;
			buffer.WriteInteger(researchEntryCount);

			// Save buffer
			buffer.Save(path);
		}

		/// <summary>
		/// Writes the modifications into the given genie file.
		/// </summary>
		/// <param name="genieFile">The genie file to be modified.</param>
		public void WriteChangesToGenieFile(GenieLibrary.GenieFile genieFile)
		{
			// Apply changes to each civ
			foreach(Civ c in genieFile.Civs)
			{
				// Apply each unit entry
				foreach(KeyValuePair<short, UnitEntry> ue in UnitEntries)
				{
					// Check whether civ has unit
					if(!c.Units.ContainsKey(ue.Key))
						continue;

					// Get corresponding unit and apply
					ue.Value.WriteChangesToGenieUnit(c.Units[ue.Key]);
				}
			}

			// Apply each research entry
			foreach(KeyValuePair<short, ResearchEntry> re in ResearchEntries)
			{
				// Get corresponding research and apply
				re.Value.WriteChangesToGenieResearch(genieFile.Researches[re.Key]);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Implementation of PropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}