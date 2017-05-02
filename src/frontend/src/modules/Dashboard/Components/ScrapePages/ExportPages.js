import React from 'react';
import Panel from '../Common/Panel';
import DateRangeForm from '../Common/DateRangeForm';

export default function ExportPages(props) {
  const extraButtonActions = [
    {title: 'Export as CSV',  onClick: () => props.onExport('csv')  },
    {title: 'Export as JSON', onClick: () => props.onExport('json') }
  ];

  return (
    <Panel title="Export">
      <DateRangeForm action="Browse" onSubmit={props.onSubmit} extraButtonActions={extraButtonActions}
                     lowerName="From" upperName="To" allowEmpty={true} />
    </Panel>
  );
}
