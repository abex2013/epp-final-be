﻿using Excellerent.Timesheet.Domain.Dtos;
using Excellerent.Timesheet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Excellerent.Timesheet.Domain.Mapping
{
    public static class ConfigurationMapping
    {
        public static object Intiger { get; private set; }

        public static List<ConfigurationEntity> MapToEntity(this ConfigurationDto configurationDto) 
        {
            List<ConfigurationEntity> configurationEntities = new List<ConfigurationEntity>();

            PropertyInfo[] properties = configurationDto.GetType().GetProperties();

            foreach (PropertyInfo property in properties) 
            {
                configurationEntities.Add(new ConfigurationEntity
                {
                    Key = property.Name,
                    Value = JsonSerializer.Serialize(property.GetValue(configurationDto, null)),
                    DataType = property.PropertyType.ToString()
                });
            }

            return configurationEntities;
        }

        public static ConfigurationDto MapToDto(this List<ConfigurationEntity> configurationEntities) 
        {
            ConfigurationDto configurationDto = new ConfigurationDto();

            PropertyInfo[] properties = configurationDto.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                ConfigurationEntity configEntity = configurationEntities.Find(config => config.Key == property.Name);

                if (configEntity == null)
                {
                    continue;
                }

                property.SetValue(configurationDto, JsonSerializer.Deserialize(configEntity?.Value, property.PropertyType));
            }

            return configurationDto;
        }
    }
}
