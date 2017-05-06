import React from 'react';
import Panel from '../Common/Panel';
import DateRangeForm from '../Common/DateRangeForm';

export default function ExportPages(props) {
  const extraButtonActions = [
    {title: 'Export as CSV',  onClick: (since, until) => props.onExport('csv', since, until)  },
    {title: 'Export as JSON', onClick: (since, until) => props.onExport('json', since, until) }
  ];

  return (
    <Panel title="Export">
      <DateRangeForm action="Browse" onSubmit={props.onSubmit} extraButtonActions={extraButtonActions}
                     lowerName="From" upperName="To" allowEmpty={true} />
    </Panel>
  );
}
